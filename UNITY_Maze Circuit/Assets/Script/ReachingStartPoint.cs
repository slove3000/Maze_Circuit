using UnityEngine;
using System.Collections;

public class ReachingStartPoint : MonoBehaviour {

    /// <summary>
    /// Instance du game manager
    /// </summary>
    private GameManager _gameManager;

    /// <summary>
    /// Instance du levelManager
    /// </summary>
    private ReachingManager _reachingManager;

    void Awake()
    {
        // Trouve le game object game manager et instancie le field
        _gameManager = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameManager>();

        if (_gameManager != null)
        {
            Debug.Log("Game Manager trouvé dans StartLine");

            _reachingManager = GameObject.Find("Reaching Manager").GetComponent<ReachingManager>();
        }
        else
        {
            Debug.Log("Game Manager pas trouvé dans StartLine");
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // La gestion de la collision n'est gérée que si le jeu est en phase de positionnement et si il reste des répétition à faire
        if (_gameManager.State == GameState.Positioning)
        {
            // Si l'objet entré en collision est le player
            if (other.name == "Player")
            {
                Debug.Log("Le player est sur la ligne de départ");
                // Alors le player est correctement placé et le countdown peut commencer
                _gameManager.SetGameState(GameState.Countdown);
            }
        }
        else if (_gameManager.State == GameState.Playing && _reachingManager.Direction == GoBack.Back)
        {
            // Signale que le retour est effectué et qu'il faut afficher une nouvelle cible.
            _reachingManager.ChangeDirection();
        }
    }
}
