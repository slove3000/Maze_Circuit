using UnityEngine;
using System.Collections;

public class CheckPointTrigger : MonoBehaviour {

    /// <summary>
    /// Instance du game manager
    /// </summary>
    private GameManager _gameManager;

    /// <summary>
    /// Instance du checkpoint manager
    /// </summary>
    private CheckpointManager _checkpointManager;

    void Awake()
    {
        // Trouve le game object game manager et instancie le field
        _gameManager = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameManager>();
        _checkpointManager = GameObject.Find("Checkpoint Manager").GetComponent<CheckpointManager>();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (_gameManager != null)
        {
            if (_gameManager.State == GameState.Playing)
            {
                if (_checkpointManager != null)
                {
                    if (col.tag == "Player")
                    {
                        _checkpointManager.NextCheckpoint();
                    }
                }
            }
        }
    }
}
