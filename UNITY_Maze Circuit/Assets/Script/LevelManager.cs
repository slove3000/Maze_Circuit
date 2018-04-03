using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour {

    /// <summary>
    /// Instance du game manager
    /// </summary>
    private GameManager _gameManager;

    /// <summary>
    /// Nombre de répétition du cycle jeu/pause
    /// </summary>
    [HideInInspector]
    public int Repetition;

    /// <summary>
    /// Temps de jeu restant pour la répétition en cours
    /// </summary>
    private float timeGameRemaining;

    /// <summary>
    /// Sommets en pixel du circuit à dessiner
    /// </summary>
    public Vector3[] PointsPixels;

    /// <summary>
    /// Ligne de départ du circuit
    /// </summary>
    public Transform StartLine;

    /// <summary>
    /// Temps que doit durer l'exercice
    /// </summary>
    public float TimeExercice;

    /// <summary>
    /// Instance du joueur
    /// </summary>
    public Transform Player;

    void Awake()
    {
        // Trouve le game object game manager et instancie le field
        _gameManager = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameManager>();

        if (_gameManager != null)
        {
            Debug.Log("Game Manager trouvé dans Level Manager");
        }
        else
        {
            Debug.Log("Game Manager pas trouvé dans Level Manager");
        }
    }

    void Start()
    {
        if (_gameManager != null)
        {
            // Positionement de la ligne de départ sur le point 0 qui est le TOUJOURS le point de départ du circuit
            if (StartLine != null && PointsPixels.Length > 0)
            {
                var positionStartLine = GameUtils.PixelToWorldPoint(PointsPixels[0], Camera.main);
                StartLine.position = positionStartLine;
            }

            // Définit le point de rotation du player
            if (this.Player != null)
            {
                this.Player.GetComponent<PlayerControl>().PointToRotateTo = this.PointsPixels[1];
            }

            // Definit le nombre de repetion en fonction de la difficulté passé au jeu
            // 0 = Baseline ou new circuit = 3 repetitions
            // 1 = Training = 20 repetitions
            if (_gameManager.Config.GameDifficulty == 0)
            {
                this.Repetition = 3;
            }
            else if (_gameManager.Config.GameDifficulty == 1)
            {
                this.Repetition = 20;
            }
            else if (_gameManager.Config.GameDifficulty == 2)
            {
                this.Repetition = 1;
            }

            // Si la phase de jeu doit durer un temps suppérieur à 0 (!= infini)
            if (this.TimeExercice > 0)
            {
                // Au debut de l'exercice le temps restant = le temps total
                this.timeGameRemaining = this.TimeExercice;
            }

            // Envoie le nombre de checkpoint a atteindre et la liste des points
            if (this.PointsPixels.Length > 0)
            {
                // Convertir les point en Vector3 en Point pour les envoyer à reaLab
                List<PointData> path = new List<PointData>();

                int maxXCentre = int.MinValue;
                int minXCentre = int.MaxValue;
                int maxYCentre = int.MinValue;
                int minYCentre = int.MaxValue;

                for (int i = 0; i < this.PointsPixels.Length; i++)
                {
                    var p = new PointData((int)this.PointsPixels[i].x, (int)this.PointsPixels[i].y);
                    path.Add(p);

                    // trouve les plus grand et plus petit x et y du circuit pour calculer le centre
                    if (this.PointsPixels[i].x > maxXCentre)
                    {
                        maxXCentre = (int)this.PointsPixels[i].x;
                    }

                    if (this.PointsPixels[i].x < minXCentre)
                    {
                        minXCentre = (int)this.PointsPixels[i].x;
                    }

                    if (this.PointsPixels[i].y > maxYCentre)
                    {
                        maxYCentre = (int)this.PointsPixels[i].y;
                    }

                    if (this.PointsPixels[i].y < minYCentre)
                    {
                        minYCentre = (int)this.PointsPixels[i].y;
                    }
                }

                if (path.Count > 0)
                {
                    // Calcul du centre
                    double centreX = (double)(maxXCentre + minXCentre) / 2.0;
                    double centreY = (double)(maxYCentre + minYCentre) / 2.0;

                    _gameManager.client.SetValue("centreX", centreX);
                    _gameManager.client.SetValue("centreY", centreY);

                    _gameManager.client.LevelLoaded(this.PointsPixels.Length);
                    _gameManager.client.SetTrajectory(path.ToArray());
                }
            }

            // Il ne faut pas passer en positioning si le niveau est celui de calibration
            if (_gameManager.Config.LevelToLoad != 9)
            {
                _gameManager.SetGameState(GameState.Positioning); 
            }
        }
    }

    void Update()
    {
        if (_gameManager.State == GameState.Playing)
        {
            // Les phases de jeu et de transistion ne sont dissponible que si il reste encore des répétitions à effectuer
            if (this.Repetition > 0)
            {
                // Le temps de l'exercice ne doit défiler que si le jeu est en cours
                // Si le temps de jeu max = 0 c'est que l'exercice n'a pas de temps limite
                if (this.TimeExercice > 0)
                {
                    // Calcule le temps restant
                    this.timeGameRemaining -= Time.deltaTime;

                    if (this.timeGameRemaining <= 0)
                    {
                        this.Repetition--;

                        this._gameManager.SetGameState(GameState.Stop);

                        if (this.Repetition > 0)
                        {
                            // Le temps de jeu est écoule il faut donc passer en phase de mouve transistion qui débutera l'effet d'apparition de l'écran de transition
                            // avant que le chrono ne déamrre pour de vrai
                            _gameManager.SetGameState(GameState.MoveTransition);
                            // Réinitialise le temps de jeu restant ainsi que la time bar
                            this.timeGameRemaining = this.TimeExercice;
                        }
                        else
                        {
                            // Après la derniere transition si le nombre de répétition est atteint il ne faut pas relancer le niveau
                            this._gameManager.SetGameState(GameState.End);
                        }
                    }
                }
            }
        }
    }
}
