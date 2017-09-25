using UnityEngine;
using System.Collections;

/// <summary>
/// Gestion de la mise en pause des animation quand le jeu passe en state de pause et
/// de la reprise quand le jeu n'est plus en state de pause
/// </summary>
public class AnimationPause : MonoBehaviour {

    /// <summary>
    /// Instance du game manager
    /// </summary>
    private GameManager _gameManager;

    private Animator animator;

    void Awake()
    {
        // Trouve le game object game manager et instancie le field
        _gameManager = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameManager>();

        if (_gameManager != null)
        {
            Debug.Log("Game Manager trouvé dans AnimationPause");

            animator = this.GetComponent<Animator>();
        }
        else
        {
            Debug.Log("Game Manager pas trouvé dans AnimationPause");
        }
    }

    void Update()
    {
        if (_gameManager.State == GameState.Pause)
        {
            animator.speed = 0;
        }
        else
        {
            if(animator.speed == 0)
                animator.speed = 1;
        }
    }
}
