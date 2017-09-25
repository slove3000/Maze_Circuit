using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour {

    /// <summary>
    /// Instance du game manager
    /// </summary>
    private GameManager _gameManager;

    /// <summary>
    /// Vitesse de rotation du joueur vers une direction
    /// </summary>
    public float RoationSpeed;

    /// <summary>
    /// Point vers lequel le joueur doit se tourner en positionnement
    /// </summary>
    [HideInInspector]
    public Vector3 PointToRotateTo;

    private float posX;
    private float posY;

    void Awake()
    {
        // Trouve le game object game manager et instancie le field
        _gameManager = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameManager>();

        if (_gameManager != null)
        {
            Debug.Log("Game Manager trouvé dans PlayerControl");
        }
        else
        {
            Debug.Log("Game Manager pas trouvé dans PlayerControl");
        }
    }

    void OnApplicationQuit()
    {
        if (_gameManager.client != null)
        {
            _gameManager.client.onNewPositions -= Client_onNewPositions;
        }
    }

    void Start()
    {
        _gameManager.client.onNewPositions += Client_onNewPositions;
    }

    private void Client_onNewPositions(object obj, PointEvent positionsArgs)
    {
        posX = positionsArgs.Point.Xf;
        posY = positionsArgs.Point.Yf;
    }

    void FixedUpdate()
    {
        // Le controle du player n'est actif que si le jeu est en phase de positionnement, de countdown, de transistion ou de jeu
        if (_gameManager.State == GameState.Positioning || _gameManager.State == GameState.Countdown || _gameManager.State == GameState.Playing || _gameManager.State == GameState.Transition || _gameManager.State == GameState.MoveTransition)
        {
            Vector3 positionPixel = new Vector3();
            Vector3 positionWorld = new Vector3();

            if (_gameManager.Config.IsDebug)
            {
                // Obtient la position de la souris sur l'écran en pixels
                positionPixel = Input.mousePosition;
                positionWorld = Camera.main.ScreenToWorldPoint(positionPixel);
            }
            else
            {
                positionPixel = new Vector3(posX, posY, 0f);
                positionWorld = GameUtils.PixelToWorldPoint(positionPixel, Camera.main);
            }

            // Redéfini le z pour l'objet player soit toujours visible par la caméra
            positionWorld.z = 0;

            // Pour que le player effectue une rotation fluide vers l'endroit ou il se dirige
            // Il faut d'abord calculer la position relative du joueur
            Vector3 relativePosition;
            // Si le jeu n'est pas en phase de playing, le player doit tourner vers le premier point de la courbe
            // Sinon lors du repositionnement par le robot, il risque d'être à l'envers 
            if (_gameManager.State != GameState.Playing)
            {
                relativePosition = GameUtils.PixelToWorldPoint(this.PointToRotateTo, Camera.main) - this.transform.position;
            }
            else
            {
                relativePosition = positionWorld - this.transform.position;
            }

            // Si le player ne bouge pas il n'y a pas besoin de changer sa rotation
            if (relativePosition != Vector3.zero)
            {
                // Calcule de l'angle de la rotation en fonction de la direction à atteindre
                float angle = Mathf.Atan2(relativePosition.y, relativePosition.x) * Mathf.Rad2Deg;
                // Soustraction de 90° car le "front" de l'objet 2d est en haut et non a droite
                angle -= 90;
                // Etat de la rotation acutelle
                var from = this.transform.rotation;
                // Calcule de la rotation à atteindre
                var to = Quaternion.AngleAxis(angle, Vector3.forward);
                // Interpolation entre la rotation actuelle et la rotation à atteindre pour que même a basse vitesse la rotation soit fluide
                transform.rotation = Quaternion.Lerp(from, to, this.RoationSpeed);
            }

            // On attribue la nouvelle position au player
            this.transform.position = positionWorld;
        }
    }
}