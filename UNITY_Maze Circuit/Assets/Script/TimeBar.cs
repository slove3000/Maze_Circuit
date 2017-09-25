using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TimeBar : MonoBehaviour {

    /// <summary>
    /// Instance du game manager
    /// </summary>
    private GameManager _gameManager;

    /// <summary>
    /// Temps restant au décompte
    /// </summary>
    private float timeRemaining;

    /// <summary>
    /// L'image de la barre de temps sur laquelle il faut changer la taille en temps réel
    /// </summary>
    public Image Bar;

    /// <summary>
    /// Temps total à écouler
    /// </summary>
    public float TotalTime;

    /// <summary>
    /// State pendant lequel le decompte se fait
    /// </summary>
    public GameState StateWhile;

    /// <summary>
    /// State pendant lequel la time bar reset
    /// </summary>
    public GameState StateToReset;

    void Awake()
    {
        // Trouve le game object game manager et instancie le field
        _gameManager = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameManager>();

        if (_gameManager != null)
        {
            Debug.Log("Game Manager trouvé dans Time Bar");
        }
        else
        {
            Debug.Log("Game Manager pas trouvé dans Time Bar");
        }
    }

    void Start()
    {
        ResetTimeBar();
    }

    void Update()
    {
        if (_gameManager.State == StateWhile)
        {
            this.timeRemaining -= Time.deltaTime;

            float fillAmount = timeRemaining / this.TotalTime;

            if (fillAmount >= 0f && fillAmount <= 1)
            {
                Bar.fillAmount = fillAmount;
            }
        }
        else if (_gameManager.State == StateToReset)
        {
            ResetTimeBar();
        }
    }

    /// <summary>
    /// Remplit la time bar au maximum
    /// </summary>
    public void ResetTimeBar()
    {
        Bar.fillAmount = 1f;
        this.timeRemaining = this.TotalTime;
    }
}
