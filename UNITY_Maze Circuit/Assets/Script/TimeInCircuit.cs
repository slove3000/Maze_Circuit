using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeInCircuit : MonoBehaviour {

	private bool inside = true;		//On commence toujours dans le circuit.
	private float timeInCircuit = 0f;

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
			Debug.Log("Game Manager trouvé dans Move Transition");
		}
		else
		{
			Debug.Log("Game Manager pas trouvé dans Transition");
		}
	}

	void Update()
	{
		if (_gameManager != null)
		{
			if (_gameManager.State == GameState.Playing && inside == true) {
				this.timeInCircuit += Time.deltaTime;
				if (timeInCircuit > 60f)
					timeInCircuit = 60f;
				_gameManager.TimeInCircuit = timeInCircuit;
			} else if ((_gameManager.State != GameState.Playing) && (_gameManager.State != GameState.Pause)) {
                inside = true;
				timeInCircuit = 0f;
			}
		}
	}

	void OnTriggerExit2D(Collider2D col)
	{
		if (_gameManager != null)
		{
			if (_gameManager.State == GameState.Playing)
			{
				if (col.tag == "Player")
				{
					inside = !inside;
					Debug.Log (inside);
				}
			}
		}
	}
}
