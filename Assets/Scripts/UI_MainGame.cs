using System.Collections;
using System.Collections.Generic;
using TMPro;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_MainGame : MonoBehaviour
{
	public Destructible player;

	public TMP_Text depthLabel;
	public TMP_Text scoreLabel;
	public TMP_Text decoysLabel;
	public TMP_Text oxygenLabel;
	public TMP_Text escapeLabel;
	public TMP_Text noOxygenLabel;

	enum GameState
	{
		InGame,
		PlayerDead
	}

	private GameState _gameState = GameState.InGame;
	private float timeToSceneTransition = -1.0f;

	public void Start()
	{
		Debug.Assert(player != null, "UI_MainGame missing reference to player!");
		Debug.Assert(GameFlow.Instance != null, "MISSING GameFlow component, should be on Globals!");
	}


	void Update()
    {
		bool isAtSurface = GameFlow.Instance.CurDepth <= 0;

		// UI update
		if (GameFlow.Instance != null)
		{
			if (depthLabel != null)
			{
				depthLabel.text = string.Format("Depth: {0} (Max: {1})", GameFlow.Instance.CurDepth, GameFlow.Instance.MaxDepth);
			}
			if (scoreLabel != null)
			{
				scoreLabel.text = string.Format("Score: {0}", GameFlow.Instance.Score);
			}
			if (decoysLabel != null)
			{
				decoysLabel.text = string.Format("Decoys Left: {0}", GameFlow.Instance.NumDecoys);
			}
			if (oxygenLabel != null)
			{
				oxygenLabel.text = string.Format("Oxygen Left: {0}", (int)GameFlow.Instance.Oxygen);
			}
			if (escapeLabel != null)
			{
				escapeLabel.enabled = isAtSurface && _gameState == GameState.InGame;
			}
		}

		// Game state update (should not be here)
		switch (_gameState)
		{
			case GameState.InGame:
			{
				if (player == null)
				{
					timeToSceneTransition = 3.0f;
					_gameState = GameState.PlayerDead;
				}
				else if (GameFlow.Instance.Oxygen <= 0.0f)
				{
					noOxygenLabel.text = "Out of Oxygen...";
					timeToSceneTransition = 3.0f;
					_gameState = GameState.PlayerDead;
				}
				else
				{
					// TODO this should not be driven by UI - Game State should be moved out of here and made cross-scene.
					bool escaped;
					GameFlow.Instance.UpdateInGame(player, out escaped);

					if (escaped)
					{
						SceneManager.LoadScene("Escaped");
					}
				}
				break;
			}
			case GameState.PlayerDead:
			{
				timeToSceneTransition -= Time.deltaTime;
				if (timeToSceneTransition <= 0.0f)
				{
					GameFlow.Instance.NumDeaths += 1;
					SceneManager.LoadScene("GameOver");
				}
				break;
			}
		}
    }
}
