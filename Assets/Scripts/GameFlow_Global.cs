using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static GameFlow_Global;

public class GameFlow_Global : MonoBehaviour
{
	private static GameFlow_Global _instance;
	public static GameFlow_Global Instance { get { return _instance; } }

	// Game config values
	public float SurfaceY;

	// Values that persist between game runs, accessible by UI
	public int HighScore { get; private set; }
	public int NumDeaths { get; private set; }
	public GameFlow_MainGame.RunState lastRunState { get; private set; }

	public void ResetToGameStart()
	{
		SceneManager.LoadScene("DetectionTestGym");
	}

	public GameFlow_Global()
	{
		HighScore = 0;
		NumDeaths = 0;
		// TODO make the UI only slow last run state if a valid one exists, so we don't have to duplicate here
		GameFlow_MainGame.RunState blankRunState;
		blankRunState.CurDepth = 0;
		blankRunState.MaxDepth = 0;
		blankRunState.Score = 0;
		blankRunState.NumDecoys = 5;
		blankRunState.Oxygen = 300;
		lastRunState = blankRunState;
	}

	void Awake()
	{
		DontDestroyOnLoad(this);
         
		if (_instance == null)
		{
			_instance = this;
		}
		else 
		{
			Destroy(gameObject);
		}
	}

	public void OnPlayerDeathTimeout(GameFlow_MainGame.RunState runState)
	{
		lastRunState = runState;
		NumDeaths += 1;
		SceneManager.LoadScene("GameOver");
	}
	public void OnPlayerEscaped(GameFlow_MainGame.RunState runState)
	{
		lastRunState = runState;
		if (runState.Score > HighScore)
		{
			HighScore = runState.Score;
		}
		SceneManager.LoadScene("Escaped");
	}
}
