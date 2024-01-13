using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameFlow : MonoBehaviour
{
	private static GameFlow _instance;
	public static GameFlow Instance { get { return _instance; } }

	// Game config values
	public float SurfaceY;

	// Persistent values
	public int HighScore = 0;
	public int NumDeaths = 0;

	// Game state values
	public int CurDepth;
	public int MaxDepth;
	public int Score;
	public int NumDecoys;
	public float Oxygen;

	public void ResetToGameStart()
	{
		CurDepth = 0;
		MaxDepth = 0;
		Score = 0;
		NumDecoys = 5;
		Oxygen = 300;
	}

	GameFlow()
	{
		ResetToGameStart();
	}

	public void UpdateInGame(Destructible player, out bool escaped)
	{
		escaped = false;
		int amountBelowSurface = (int)(SurfaceY - player.transform.position.y);

		if (amountBelowSurface >= 0)
		{
			CurDepth = amountBelowSurface;
			if (CurDepth > MaxDepth)
			{
				MaxDepth = CurDepth;
			}
		}
		if (amountBelowSurface <= 0)
		{
			var gamepad = Gamepad.current;
			if (gamepad != null)
			{
				escaped = gamepad.startButton.wasPressedThisFrame;

				if (escaped)
				{
					if (Score > HighScore)
					{
						HighScore = Score;
					}
				}
			}
		}
		Oxygen -= Time.deltaTime;
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
}
