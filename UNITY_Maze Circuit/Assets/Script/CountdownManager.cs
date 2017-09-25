using UnityEngine;
using System.Collections;

public class CountdownManager : MonoBehaviour {

    /// <summary>
    /// Temps du decompte
    /// </summary>
    public float CountdownTime;

    /// <summary>
    /// State pendant lequel le decompte se fait
    /// </summary>
    public GameState StateWhile;

    /// <summary>
    /// State que le jeu aura après le countdown
    /// </summary>
    public GameState StateToGo;

    /// <summary>
    /// Instance du game manager
    /// </summary>
    private GameManager _gameManager;

    /// <summary>
    /// Temps restant au décompte
    /// </summary>
    private float timeRemaining;

    void Awake()
    {
        // Trouve le game object game manager et instancie le field
        _gameManager = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameManager>();

        if (_gameManager != null)
        {
            Debug.Log("Game Manager trouvé dans CountdownManager");
        }
        else
        {
            Debug.Log("Game Manager pas trouvé dans CountdownManager");
        }
    }

    void Start()
    {
        this.timeRemaining = CountdownTime;
    }

    void Update()
    {
        if (_gameManager.State == this.StateWhile)
        {
            this.timeRemaining -= Time.deltaTime;
            if (this.timeRemaining <= 0)
            {
                _gameManager.SetGameState(this.StateToGo);
                Destroy(this.gameObject);
            }
        }
    }
}
