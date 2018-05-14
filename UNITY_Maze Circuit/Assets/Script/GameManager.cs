using UnityEngine;
using System.Collections;
using System;
using System.IO;
using UnityEngine.SceneManagement;

public enum GameState
{
    Init,
    LevelLoading,
    Positioning,
    Countdown,
    Playing,
    MoveTransition,
    Transition,
    Pause,
    Stop,
    End
}

public class GameManager : MonoBehaviour
{
    #region Fields
    /// <summary>
    /// Instance courante du game manager
    /// </summary>
    private static GameManager _instance = null;

    /// <summary>
    /// Etat actuel du jeu
    /// </summary>
    private GameState _state;

    /// <summary>
    /// Instance du state précédent
    /// </summary>
    private GameState _previousState;

    /// <summary>
    /// Permet de savoir si on est en pause
    /// </summary>
    private bool pause = false;
    #endregion

    #region Properties
    /// <summary>
    /// Gets l'instance courante du game manager
    /// </summary>
    public static GameManager Instance { get { return _instance; } }

    /// <summary>
    /// Gets l'état actuel du jeu
    /// </summary>
    public GameState State { get { return _state; } }

    [HideInInspector]
    public ReaLabCommunication client;

    public ReaLabConfiguration Config { get; set; }
    [HideInInspector]
    public Color ColorFond;
    [HideInInspector]
    public Color ColorCurseur;

    [HideInInspector]
	public bool inReaching = false;
	[HideInInspector]
	public float TimeInCircuit = 0f;
	[HideInInspector]
	public int HighScore = 0;
	[HideInInspector]
	public int SegmentDone = 0;

    private bool canStartGame = false;

    private bool canGameStop = false;

    private bool canDoPause = false;
    #endregion

    void Awake()
    {

        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(this.gameObject);
        }

        // Le game manager ne sera pas détruit au chargement d'une nouvelle scène
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        this.client = GameObject.FindGameObjectWithTag("Communication").GetComponent<ReaLabCommunication>();
        this.client.Connect();
        this.client.onPause += Client_onPause;
        this.client.onNewConfiguration += Client_onNewConfiguration;
        this.client.onStop += Client_onStop;
    }

    private void Client_onStop(object obj, EventArgs stopArgs)
    {
        this.client.Disconnect();
        this.client.onPause -= Client_onPause;
        this.client.onNewConfiguration -= Client_onNewConfiguration;
        this.client = null;

        this.canGameStop = true;
    }

    void OnApplicationQuit()
    {
        Debug.Log("Quit");
        if (this.client != null)
        {
            Debug.Log("Disco Quit");
            this.client.Disconnect();
            this.client.onPause -= Client_onPause;
            this.client.onNewConfiguration -= Client_onNewConfiguration;
            this.client = null;
        }
    }

    void Update()
    {
        if (this.canStartGame == true)
        {
            this.canStartGame = false;
            this.StartGame(); 
        }

        if (canGameStop == true)
        {
            canGameStop = false;
            Application.Quit();
        }

        if (canDoPause == true)
        {
            if (this.State != GameState.Pause && this.State != GameState.Stop)
            {
                this.SetGameState(GameState.Pause); 
            }
        }
        else
        {
            if (this.State == GameState.Pause)
            {
                this.SetGameState(this._previousState);
            }
        }
    }

    private void Client_onNewConfiguration(object obj, ConfigEvent configArgs)
    {
        if (this.Config == null)
        {
            this.Config = configArgs.Config;
            this.canStartGame = true; 
        }
    }

    void StartGame()
    {
        this.SetGameState(GameState.Init);
        
        try
        {
            if (this.Config != null)
            {
                this.SetGameState(GameState.LevelLoading);

                //Lecture de la couleur demandée
                object color = client.GetValue("Color");
                if (color != null)
                {
                    int newcolor = int.Parse(color.ToString());
                    switch (newcolor)
                    {
                        case 1:
                            ColorFond = new Color(49f / 255f, 77f / 255f, 121f / 255f, 0f);
                            ColorCurseur = new Color(0f / 255f, 132f / 255f, 187f / 255f, 1f);
                            break;
                        case 2:
                            ColorFond = new Color(255f / 255f, 255f / 255f, 255f / 255f, 0f);
                            ColorCurseur = new Color(218f / 255f, 0f / 255f, 0f / 255f, 1f);
                            break;
                        case 3:
                            ColorFond = new Color(107f / 255f, 177f / 255f, 255f / 255f, 0f);
                            ColorCurseur = new Color(218f / 255f, 0f / 255f, 0f / 255f, 1f);
                            break;
                        default:
                            ColorFond = new Color(49f / 255f, 77f / 255f, 121f / 255f, 0f);
                            ColorCurseur = new Color(255f / 255f, 255f / 255f, 0f / 255f, 1f);
                            break;
                    }
                }
                else
                {
                    ColorFond = new Color(49f / 255f, 77f / 255f, 121f / 255f, 0f);
                    ColorCurseur = new Color(0f / 255f, 132f / 255f, 187f / 255f, 1f);
                }

                if (this.Config.LevelToLoad < 10)
                {
					inReaching = false;
                    SceneManager.LoadScene("circuit" + this.Config.LevelToLoad);

                    if (this.Config.LevelToLoad == 9)
                    {
                        this.SetGameState(GameState.Playing);
                    }
                }
                else
                {
					inReaching = true;
                    SceneManager.LoadScene("reaching");
                }
            }
            else
            {
                Debug.Log("null");
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    /// <summary>
    /// Change le state du jeu
    /// </summary>
    /// <param name="newState">nouveau state que le jeu doit adopter</param>
    public void SetGameState(GameState newState)
    {
        // Ne pas écraser le state précedent si le même état est lancé plusieur fois de suite
        if (newState != this._state)
        {
            this._previousState = this._state;
        }

        this._state = newState;

        switch (newState)
        {
            case GameState.Init:
                Debug.Log("-----Init-----");
                break;
            case GameState.LevelLoading:
                Debug.Log("-----Level Loading-----");
                break;
            case GameState.Positioning:
                Debug.Log("-----Positioning-----");
                break;
            case GameState.Countdown:
                Debug.Log("-----Countdown-----");
                if (this._previousState != GameState.Pause) // Ne pas charger plusieur fois la scene si on sort d'une pause pendant la scene originale
                {
                    SceneManager.LoadScene("countdown", LoadSceneMode.Additive);
                }
                break;
            case GameState.Playing:
                this.client.LevelStarted();
                Debug.Log("-----Playing-----");
                break;
            case GameState.MoveTransition: // Déplacement de l'écran de transition avant que le chrono ne démarre pour de vrai
                Debug.Log("-----MoveTransition-----");
                if (this._previousState != GameState.Pause) // Ne pas charger plusieur fois la scene si on sort d'une pause pendant la scene originale
                {
                    SceneManager.LoadScene("transition", LoadSceneMode.Additive);
                }
                break;
            case GameState.Transition:
                Debug.Log("-----Transition-----");
                break;
            case GameState.Pause:
                if (this._previousState != GameState.Pause) // Ne pas charger plusieur fois la scene si on sort d'une pause pendant la scene originale
                {
                    SceneManager.LoadScene("pause", LoadSceneMode.Additive);
                }
                Debug.Log("-----Pause-----");
                break;
            case GameState.Stop:
                this.client.LevelStopped();
                Debug.Log("-----Stop-----");
                break;
            case GameState.End:
                Debug.Log("-----End-----");
                if (this._previousState != GameState.Pause) // Ne pas charger plusieur fois la scene si on sort d'une pause pendant la scene originale
                {
                    SceneManager.LoadScene("end", LoadSceneMode.Additive);
                }
                break;
            default:
                break;
        }
    }

    private void Client_onPause(object obj, EventArgs pauseArgs)
    {
        if (!pause)
        {
            if (this.State != GameState.Pause && this.State != GameState.Stop)
            {
                this.canDoPause = true;
                //this.SetGameState(GameState.Pause);
                pause = !pause;
            }
        }
        else
        {
            if (this.State == GameState.Pause)
            {
                this.canDoPause = false;
                //this.SetGameState(this._previousState);
                pause = !pause;
            }
        }
    }
}

public static class GameUtils
{
    /// <summary>
    /// Converti un point en pixel 1920*1080 en world point en tenant compte de la zone de jeu pour que la convertion soit bonne aussi dans l'éditeur
    /// </summary>
    /// <param name="point">Le point en pixel</param>
    /// <param name="cam">La caméra de la scene</param>
    /// <returns>Le point en world point</returns>
    public static Vector3 PixelToWorldPoint(Vector3 point, Camera cam)
    {
        // La conversion des pixel en world point dépend de la zone visible par la caméra
        // Cette zone est l'écran de jeu dont la taille peut varier d'un éditeur à l'autre
        // Pour que la conversion se fasse correctement indépendament de la taille de la zone de jeu 
        // Il faut mettre à l'echelle de la caméra les point en pixel utilisé. La résolution de base est 1920*1080

        var rectCam = cam.pixelRect;

        float xEchelle = ((float)point.x / 1920f) * rectCam.width;
        // Le 0;0 de Unity (bas gauche) n'est pas le même que celui d'une fenetre classique (haut gauche)
        // pour ramener le point dans le bon repère il faut faire 1080 - coord y
        float yEchelle = ((1080f - (float)point.y) / 1080f) * rectCam.height;
        var pixelEchelle = new Vector3(xEchelle, yEchelle, 0f);

        var worldPoint = cam.ScreenToWorldPoint(pixelEchelle);
        // Redéfini le z pour l'objet soit toujours visible par la caméra
        worldPoint.z = 0;

        return worldPoint;
    }
}
