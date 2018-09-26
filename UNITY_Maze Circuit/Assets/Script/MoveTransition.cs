using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MoveTransition : MonoBehaviour {

    /// <summary>
    /// Vitesse de la translation
    /// </summary>
    public float TransitionSpeed;

    /// <summary>
    /// Point de départ du mouvement
    /// </summary>
    public Transform StartPoint;

    /// <summary>
    /// Point d'arrivé du mouvement
    /// </summary>
    public Transform EndPoint;

    /// <summary>
    /// Direction du mouvement
    /// </summary>
    public Vector3 Direction;

    /// <summary>
    /// Instance du game manager
    /// </summary>
    private GameManager _gameManager;

    public GameState StateWhile;

    public Text ScoreTxt;
    private int score;
    private float tempScore;

    public Text HighScoreTxt;
    private int highScore = 0;
    private float tempHighScore;

    private bool needToCheck = true;

    void Awake()
    {
        // Trouve le game object game manager et instancie le field
        _gameManager = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameManager>();
        
        if (_gameManager != null)
        {
            Debug.Log("Game Manager trouvé dans Move Transition");
        }
        else
        {
            Debug.Log("Game Manager pas trouvé dans Transition");
        }
    }

    void Start()
    {
        this.transform.position = this.StartPoint.position;
    }

	// Update is called once per frame
	void Update ()
    {
		if (_gameManager.inReaching == true) {
			// En reaching le calcul du score est fait par REALab
			// Regarde si le score et high score a ete mit a jour
			if (needToCheck == true) {
				object s = _gameManager.client.GetValue ("Score");
				if (s != null) {
					score = int.Parse (s.ToString ());
				} else {
					score = 100;
				}

				object hs = _gameManager.client.GetValue ("HighScore");
				if (hs != null) {
					highScore = int.Parse (hs.ToString ());
					needToCheck = false;
				} else {
					highScore = 100;
				}
			}
		} 
		else 
		{
			// Calcul du score et high score
			score = (int)(((_gameManager.TimeInCircuit / 60f) * _gameManager.SegmentDone) * 10);

            if (score > _gameManager.HighScore)
            {
                _gameManager.HighScore = score;
                highScore = score;
            }
            else
            {
                highScore = _gameManager.HighScore;
            }
				

			Debug.Log("Score = (" + _gameManager.TimeInCircuit + "/60) * " + _gameManager.SegmentDone + " * 100");
            Debug.Log("High Score = " + highScore);
		}

        // Changement du score
        tempScore = Mathf.MoveTowards(tempScore, score, (float)((float)score/15f) * Time.deltaTime);

        if (this.ScoreTxt != null)
        {
            ScoreTxt.text = Mathf.RoundToInt(tempScore).ToString();
        }

        tempHighScore = Mathf.MoveTowards(tempHighScore, highScore, (float)((float)highScore / 15f) * Time.deltaTime);
        HighScoreTxt.text = Mathf.RoundToInt(tempHighScore).ToString();

        // L'ecran de transition ne doit se deplacer que dans le state de move transistion
        // Ou bien si le jeu est en stop. Car a la fin des répétitions le jeu passe en stop mais l'écran de transition doît quand même apparaître
        if (_gameManager.State == StateWhile)
        {
            if (this.transform.position.y > this.EndPoint.position.y)
            {
                this.transform.Translate(this.Direction * TransitionSpeed * Time.deltaTime);
            }
            else
            {
                // Replace précisement l'objet a la bonne position
                this.transform.position = this.EndPoint.position;

                if (_gameManager.State == GameState.MoveTransition)
                {
                    // Il ne faut pas passer en transistion si le jeu est en stop
                    _gameManager.SetGameState(GameState.Transition);

                    // Il faut détruire les points pour qu'il ne reste pas dans la scene après la transition
                    Destroy(this.StartPoint.gameObject);
                    Destroy(this.EndPoint.gameObject);
                }
            }
        }
	}
}
