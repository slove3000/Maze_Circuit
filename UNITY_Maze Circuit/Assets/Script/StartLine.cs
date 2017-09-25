using UnityEngine;
using System.Collections;

public class StartLine : MonoBehaviour {

    /// <summary>
    /// Instance du game manager
    /// </summary>
    private GameManager _gameManager;

    /// <summary>
    /// Instance du levelManager
    /// </summary>
    private LevelManager _levelManager;

    void Awake()
    {
        // Trouve le game object game manager et instancie le field
        _gameManager = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameManager>();

        if (_gameManager != null)
        {
            Debug.Log("Game Manager trouvé dans StartLine");

            _levelManager = GameObject.Find("Level Manager").GetComponent<LevelManager>();
        }
        else
        {
            Debug.Log("Game Manager pas trouvé dans StartLine");
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // La gestion de la collision n'est gérée que si le jeu est en phase de positionnement et si il reste des répétition à faire
        if (_gameManager.State == GameState.Positioning && _levelManager.Repetition > 0)
        {
            // Si l'objet entré en collision est le player
            if (other.name == "Player")
            {
                Debug.Log("Le player est sur la ligne de départ");
                // Alors le player est correctement placé et le countdown peut commencer
                _gameManager.SetGameState(GameState.Countdown);
            }
        }
    }
}
