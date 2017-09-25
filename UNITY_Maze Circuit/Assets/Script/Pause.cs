using UnityEngine;
using System.Collections;

public class Pause : MonoBehaviour {

    /// <summary>
    /// Instance du game manager
    /// </summary>
    private GameManager _gameManager;

    void Awake()
    {
        // Trouve le game object game manager et instancie le field
        _gameManager = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameManager>();

        if (_gameManager != null)
        {
            Debug.Log("Game Manager trouvé dans Pause");
        }
        else
        {
            Debug.Log("Game Manager pas trouvé dans Pause");
        }
    }

    // Update is called once per frame
    void Update ()
    {
        if (_gameManager.State != GameState.Pause)
        {
            Destroy(this.gameObject);
        }
	}
}
