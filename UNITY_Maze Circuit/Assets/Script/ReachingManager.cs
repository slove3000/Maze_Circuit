using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum GoBack
{
    Go,
    Back
}

public class ReachingManager : MonoBehaviour {

    /// <summary>
    /// Instance du game manager
    /// </summary>
    private GameManager _gameManager;

    /// <summary>
    /// Instance du joueur
    /// </summary>
    public Transform Player;

    /// <summary>
    /// Les différante cible à atteindre
    /// </summary>
    public GameObject[] Targets;


    /// <summary>
    /// Cible actuelement utilisé par l'exercice
    /// </summary>
    private GameObject currentTarget;

    /// <summary>
    /// Cible affiché avant la cible actuelle
    /// </summary>
    private GameObject previousTarget;

    /// <summary>
    /// Permet de savoir si on est en allé ou en retour
    /// </summary>
    [HideInInspector]
    public GoBack Direction;

    /// <summary>
    /// Nombre de fois que les cibles peuvent apparaître
    /// </summary>
    public int Repetition;

    /// <summary>
    /// Les différantes cible restante à afficher
    /// </summary>
    private List<GameObject> targetsLeft;

    /// <summary>
    /// Objet servant à faire le random
    /// </summary>
    private System.Random rand;

    /// <summary>
    /// Point de départ de toutes les targets
    /// </summary>
    private Transform startingPoint;

    /// <summary>
    /// Temps écoulé entre deux checkpoint (durée du segment)
    /// </summary>
    private float timeElpase = 0f;

    void Awake()
    {
        // Trouve le game object game manager et instancie le field
        _gameManager = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameManager>();

        if (_gameManager != null)
        {
            Debug.Log("Game Manager trouvé dans reaching Manager");
            this.startingPoint = GameObject.Find("Starting Point").GetComponent<Transform>();
        }
        else
        {
            Debug.Log("Game Manager pas trouvé dans reaching Manager");
        }
    }

    void Update()
    {
        if (_gameManager != null)
        {
            if (_gameManager.State == GameState.Playing)
            {
                this.timeElpase += Time.deltaTime;
            }
        }
    }

    void Start()
    {
        if (this.Targets != null)
        {
            // Remplit la liste de choix restant avec l'entiereté des cible possible puisque l'exercice n'à pas encore commencé et que toutes les cibles peuvent donc être choisie
            this.targetsLeft = new List<GameObject>(this.Targets);

            // Calcul du centre du circuit
            // Il faut prendre le plus petit et le plus grand x,y et faire la moyenne
            int minX = int.MaxValue;
            int maxX = int.MinValue;
            int minY = int.MaxValue;
            int maxY = int.MinValue;
            foreach (var item in this.Targets)
            {
                var vectPos = item.transform.Find("Ending Point").position;

                if (vectPos.x > maxX)
                {
                    maxX = (int)vectPos.x;
                }
                if (vectPos.x < minX)
                {
                    minX = (int)vectPos.x;
                }

                if (vectPos.y > maxY)
                {
                    maxY = (int)vectPos.y;
                }
                if (vectPos.y < minY)
                {
                    minY = (int)vectPos.y;
                }
            }

            var vectCentre = Camera.main.WorldToScreenPoint(new Vector3((maxX + minX) / 2, (maxY + minY) / 2, 0f));
            _gameManager.client.SetValue("centreX", vectCentre.x);
            _gameManager.client.SetValue("centreY", vectCentre.y);

            this.rand = new System.Random();

            // Choix de la premiere cible random à afficher
            this.PickATarget();

            this.Direction = GoBack.Go;

            var ending = GameObject.Find("Ending Point");

            if (ending != null)
            {
                // Définit le point de rotation du player
                if (this.Player != null)
                {
                    // reconverti les world point en pixel pour les passer au player
                    var pixels = Camera.main.WorldToScreenPoint(ending.transform.position);
                    pixels.x = (pixels.x * 1920) / Camera.main.pixelRect.width;
                    pixels.y = Math.Abs(((pixels.y * 1080) / Camera.main.pixelRect.height) - 1080);
                    this.Player.GetComponent<PlayerControl>().PointToRotateTo = pixels;
                }
            }

            var path = this.GetAllSegments();
            _gameManager.client.SetTrajectory(path);

            _gameManager.SetGameState(GameState.Positioning);
        }
    }

    private PointData[] GetAllSegments()
    {
        // Calcul de tous les segments à effectuer pour les envoyer en une fois au robot
        // Convertir les point en Vector3 en Point pour les envoyer à reaLab
        List<PointData> path = new List<PointData>();

        // Ajoute la cible courrante pour que le robot sache par quel segment commmencer. Après le premier posisionnement il n'est plus utilisé
        // Ajout du point de départ
        var vectStart = Camera.main.WorldToScreenPoint(new Vector3(this.startingPoint.position.x, this.startingPoint.position.y, 0f));
        var start = new PointData(vectStart.x, 1080 - vectStart.y);
        path.Add(start);

        // Ajout du point d'arrivé
        var posEnd = this.currentTarget.transform.Find("Ending Point").position;
        var vectEnd = Camera.main.WorldToScreenPoint(new Vector3(posEnd.x, posEnd.y, 0f));
        var end = new PointData(vectEnd.x, 1080 - vectEnd.y);
        path.Add(end);

        foreach (var target in this.Targets)
        {
            // Ajout du point de départ
            vectStart = Camera.main.WorldToScreenPoint(new Vector3(this.startingPoint.position.x, this.startingPoint.position.y, 0f));
            start = new PointData(vectStart.x, 1080 - vectStart.y);
            path.Add(start);

            // Ajout du point d'arrivé
            posEnd = target.transform.Find("Ending Point").position;
            vectEnd = Camera.main.WorldToScreenPoint(new Vector3(posEnd.x, posEnd.y, 0f));
            end = new PointData(vectEnd.x, 1080 - vectEnd.y);
            path.Add(end);
        }

        return path.ToArray();
    }

    /// <summary>
    /// Trouve l'indice de la target actuel pour dire au robot par quel segement commencer
    /// </summary>
    private int GetCurrentTarget()
    {
        var indiceCurrentTarget = 0;
        for (int i = 0; i < Targets.Length; i++)
        {
            if (Targets[i] == currentTarget)
            {
                indiceCurrentTarget = i;
            }
        }

        return indiceCurrentTarget;
    }

    /// <summary>
    /// Choix d'une cible pseudo random
    /// Chaque cible se repete plusieur fois
    /// </summary>
    void PickATarget()
    {
        if (this.targetsLeft != null)
        {
            // Si il reste des cible à afficher pour cette répétiton
            if (this.targetsLeft.Count > 0)
            {
                // Désactive la cible affichée pour le moment
                if (this.currentTarget != null)
                {
                    this.currentTarget.SetActive(false);
                }

                bool same = true;
                int indice = 0;

                while (same == true)
                {
                    // Choisit une cible random
                    indice = rand.Next(0, this.targetsLeft.Count);

                    // Vérifié que la cible random choisie n'est pas la même qu'avant.
                    // C'est possible uniquement si la derniere d'une répétition est la même que la premiere d'une nouvelle répétition
                    if (this.currentTarget != this.targetsLeft[indice])
                    {
                        this.currentTarget = this.targetsLeft[indice];
                        same = false;
                    }
                }
                
                // Supprime de la liste la cible qui vient d'être affichée pour qu'elle ne le soit pas une deuxieme fois lors de cette répétition
                this.targetsLeft.RemoveAt(indice);

                // Affiche la nouvelle cible courrante
                this.currentTarget.SetActive(true);

                int angle = 0;

                for (int i = 0; i < Targets.Length; i++)
                {
                    if (Targets[i] == currentTarget)
                    {
                        switch (i)
                        {
                            case 0:
                                angle = 30;
                                break;
                            case 1:
                                angle = 60;
                                break;
                            case 2:
                                angle = -30;
                                break;
                            case 3:
                                angle = -60;
                                break;
                            default:
                                break;
                        }
                    }
                }
                _gameManager.client.SetValue("Angle", angle);
            }
            else
            {
                this.Repetition--;

                if (this.Repetition > 0)
                {
                    if (this.Targets != null)
                    {
                        // Si le nombre de répétition n'est pas fini et qu'il ne reste plus de cible dans la liste
                        // C'est qu'il faut reset les choix possible parce que toutes les cible ont été affiché au moins une fois
                        this.targetsLeft = new List<GameObject>(this.Targets);
                        PickATarget();
                    }
                }
                else
                {
                    _gameManager.SetGameState(GameState.Stop);
                    this._gameManager.SetGameState(GameState.End);
                }
            }
        }
    }

    /// <summary>
    /// Inverse le sens de la direction
    /// </summary>
    public void ChangeDirection()
    {
        _gameManager.client.SetValue("TempsSeg", this.timeElpase);
        Debug.Log("Temps Segment = " + this.timeElpase);
        this.timeElpase = 0f;

        if (this.Direction == GoBack.Go)
        {
            Debug.Log("Back");
            this.Direction = GoBack.Back;
            int indiceToSend = 2 + (2 * this.GetCurrentTarget());
            _gameManager.client.CheckpointReached(indiceToSend + 1);
        }
        else
        {
            Debug.Log("Go");
            this.Direction = GoBack.Go;

            // Reset de la zone de validation d'une target après qu'elle ai été validé
            var targetScript = this.currentTarget.GetComponent<Target>();
            if (targetScript != null)
            {
                targetScript.SliderTime.value = 0;
            }

            PickATarget();
            int indiceToSend = 2 + (2 * this.GetCurrentTarget());
            _gameManager.client.CheckpointReached(indiceToSend);
        }
    }
}
