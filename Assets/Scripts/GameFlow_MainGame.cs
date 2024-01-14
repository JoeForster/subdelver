using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

public class GameFlow_MainGame : MonoBehaviour
{
	// TODO we could almost do away with the need for this singleton by using events
	private static GameFlow_MainGame _instance;
	public static GameFlow_MainGame Instance { get { return _instance; } }

	// Main game flow state: since this is just within the game scene,
	// it is just in-game or dead pending scene transition to gameover.
	public enum GameState
	{
		InGame,
		PlayerDead
	}
	public GameState State { get; private set; } = GameState.InGame;

	// Required game component references
	public Destructible player;

	// Game config values
	public float SurfaceY;
	
	// Per-run game state values, readable by UI
	public struct RunState
	{
		// TODO: Make initial values configurable
		public int CurDepth;
		public int MaxDepth;
		public int Score;
		public int NumDecoys;
		public float Oxygen;
	}

	public RunState runState;

	// Private state
	private float timeToSceneTransition = -1.0f;

	void Awake()
	{
		if (_instance == null)
		{
			_instance = this;
		}
		else 
		{
			Destroy(gameObject);
		}

		runState.CurDepth = 0;
		runState.MaxDepth = 0;
		runState.Score = 0;
		runState.NumDecoys = 5;
		runState.Oxygen = 300;
	}

	void Update()
	{
		// Game state update (should not be here - should be managed by GameFlow or a new component?)
		switch (State)
		{
			case GameState.InGame:
			{
				if (player == null)
				{
					timeToSceneTransition = 3.0f;
					State = GameState.PlayerDead;
				}
				else if (runState.Oxygen <= 0.0f)
				{
					timeToSceneTransition = 3.0f;
					State = GameState.PlayerDead;
				}
				else
				{
					UpdateInGame(player);
				}
				break;
			}
			case GameState.PlayerDead:
			{
				timeToSceneTransition -= Time.deltaTime;
				if (timeToSceneTransition <= 0.0f)
				{
					GameFlow_Global.Instance.OnPlayerDeathTimeout(runState);
				}
				break;
			}
		}
	}

	public void UpdateInGame(Destructible player)
	{
		bool escaped = false;
		int amountBelowSurface = (int)(SurfaceY - player.transform.position.y);

		if (amountBelowSurface >= 0)
		{
			UpdateDepth(amountBelowSurface);
		}
		if (amountBelowSurface <= 0)
		{
			var gamepad = Gamepad.current;
			if (gamepad != null)
			{
				escaped = gamepad.startButton.wasPressedThisFrame;
			}
		}

		if (escaped)
		{
			GameFlow_Global.Instance.OnPlayerEscaped(runState);
		}

		runState.Oxygen -= Time.deltaTime;
	}

	public void OnCollectItem(int ScoreValue, int DecoyValue, float OxygenValue)
	{
		runState.Score += ScoreValue;
		runState.NumDecoys += DecoyValue;
		runState.Oxygen += OxygenValue;
	}

	public void OnConsumeDecoy()
	{
		runState.NumDecoys--;
	}

	public void UpdateDepth(int amountBelowSurface)
	{
		runState.CurDepth = amountBelowSurface;
		if (runState.CurDepth > runState.MaxDepth)
		{
			runState.MaxDepth = runState.CurDepth;
		}
	}
}
