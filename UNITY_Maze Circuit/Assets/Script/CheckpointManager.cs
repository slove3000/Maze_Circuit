using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CheckpointManager : MonoBehaviour {

    /// <summary>
    /// Instance du game manager
    /// </summary>
    private GameManager _gameManager;

    /// <summary>
    /// Liste des checkpoints à gérer
    /// </summary>
    private List<Transform> checkpoints = new List<Transform>();

    /// <summary>
    /// Indice du checkpoint qui doit être traversé
    /// L'indice est à 1 en commencant car le 0 est sur la ligne de départ et ne doit pas être utilisé au premier tour
    /// </summary>
    private int currentCheckpoint = 1;

    /// <summary>
    /// Temps écoulé entre deux checkpoint (durée du segment)
    /// </summary>
    private float timeElpase = 0f;

	private int segmentDone = 0;

	private bool reseted = false;

    void Awake()
    {
        // Trouve le game object game manager et instancie le field
        _gameManager = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameManager>();

        if (_gameManager != null)
        {
            Debug.Log("Game Manager trouvé dans CountdownManager");

            // Récupère tous les checkpoint contenu dans le game object
            foreach (Transform child in this.transform)
            {
                checkpoints.Add(child);
                // On désactive tous les checkpoint car seulement un seul à la fois peut être actif
                child.gameObject.SetActive(false);
            }

            // Active le checkpoint 1 (pas le 0 qui est sur la ligne de départ)
            this.ManageCheckpointState(true, currentCheckpoint);
        }
        else
        {
            Debug.Log("Game Manager pas trouvé dans CountdownManager");
        }
    }

    void FixedUpdate()
    {
        if (_gameManager != null)
        {
			if (_gameManager.State == GameState.MoveTransition && reseted == false)
            {
                // Reset de l'état des checkpoint pour préparer la nouvelle ittération du circuit
                ResetCheckpoint();
				reseted = true;
            }
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

    private void ManageCheckpointState(bool status, int checkpoint)
    {
        if (checkpoints.Count >= checkpoint)
        {
            checkpoints[checkpoint].gameObject.SetActive(status);
        }
    }

    private void ResetCheckpoint()
    {
        ManageCheckpointState(false, currentCheckpoint);
        currentCheckpoint = 1;
        ManageCheckpointState(true, currentCheckpoint);
		segmentDone = 0;
    }

    /// <summary>
    /// Active le checkpoint suivant
    /// </summary>
    public void NextCheckpoint()
    {
        if (_gameManager != null)
        {
            if (_gameManager.State == GameState.Playing)
            {
				reseted = false;
                _gameManager.client.SetValue("TempsSeg", this.timeElpase);
                Debug.Log("Temps Segment = " + this.timeElpase);
                this.timeElpase = 0f;

                // Signal à REAlab qu'un segement est terminé
                _gameManager.client.CheckpointReached(currentCheckpoint);

                // Désactive le checkpoint qui vient d'être passé
                ManageCheckpointState(false, currentCheckpoint);
                // Passe au checkpoint suivant
                currentCheckpoint++;

                if (currentCheckpoint >= checkpoints.Count)
                {
                    currentCheckpoint = 0;
                }

                // Active le checkpoint suivant
                ManageCheckpointState(true, currentCheckpoint);

				segmentDone++;
				_gameManager.SegmentDone = segmentDone;
                Debug.Log("Next Checkpoint");
				Debug.Log ("SegmentDone : " + segmentDone);
            }
        }
    }
}
