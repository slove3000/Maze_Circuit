using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Target : MonoBehaviour {

    /// <summary>
    /// Instance du game manager
    /// </summary>
    private GameManager _gameManager;

    /// <summary>
    /// Instance du levelManager
    /// </summary>
    private ReachingManager _reachingManager;

    /// <summary>
    /// Point d'arrivé
    /// </summary>
    public Transform EndingPoint;

    /// <summary>
    /// Temps qu'il faut rester pour avoir la validation
    /// </summary>
    public float TimeToStay;

    /// <summary>
    /// Slider qui sert a remplit le rond
    /// </summary>
    public Slider SliderTime;

    /// <summary>
    /// Image qui est remplit en fonction du temps
    /// </summary>
    public Image FillImage;

    /// <summary>
    /// Couleur quand le pourcentage est bas
    /// </summary>
    public Color NoTimeColor = Color.red;

    /// <summary>
    /// Couleur quand le pourcentage est haut
    /// </summary>
    public Color FullTimeColor = Color.green;

    /// <summary>
    /// Permet de savoir si le player est dans la zone de validation
    /// </summary>
    private bool isInZone = false;

    /// <summary>
    /// Temps restant a valider le point
    /// </summary>
    private float timeRemaining;

    void Awake()
    {
        // Trouve le game object game manager et instancie le field
        _gameManager = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameManager>();

        if (_gameManager != null)
        {
            Debug.Log("Game Manager trouvé dans target");

            _reachingManager = GameObject.Find("Reaching Manager").GetComponent<ReachingManager>();
        }
        else
        {
            Debug.Log("Game Manager pas trouvé dans target");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_gameManager.State == GameState.Playing)
        {
            // Si l'objet entré en collision est le player
            if (other.name == "Player")
            {
                if (_reachingManager.Direction == GoBack.Go)
                {
                    timeRemaining = this.TimeToStay;
                    this.isInZone = true;
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (_gameManager.State == GameState.Playing)
        {
            // Si l'objet entré en collision est le player
            if (other.name == "Player")
            {
                if (_reachingManager.Direction == GoBack.Go)
                {
                    this.isInZone = false;
                    timeRemaining = this.TimeToStay;
                }
            }
        }
    }

    void Update()
    {
        if (this._gameManager.State == GameState.Playing)
        {
            if (this.isInZone == true && this._reachingManager.Direction == GoBack.Go)
            {
                this.timeRemaining -= Time.deltaTime;

                // Calcul du % de temps passé dans la zone pour le définir dans le slider
                var prct = 1 - (this.timeRemaining / this.TimeToStay);

                this.SliderTime.value = prct;

                this.FillImage.color = Color.Lerp(this.NoTimeColor, this.FullTimeColor, prct);

                // La validation est terminée
                if (this.timeRemaining <= 0)
                {
                    this.isInZone = false;
                    timeRemaining = this.TimeToStay;
                    this._reachingManager.ChangeDirection();
                }
            }
        }
    }
}
