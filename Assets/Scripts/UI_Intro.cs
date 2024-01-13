using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UI_Intro : MonoBehaviour
{
	public TMP_Text maxDepthText;
	public TMP_Text scoreText;
	public TMP_Text highScoreText;
	public TMP_Text numDeathsText;


	void Update()
    {
		Debug.Assert(GameFlow.Instance != null, "No GameFlow!");
		if (GameFlow.Instance != null)
		{
			
			if (maxDepthText != null)
			{
				maxDepthText.text = string.Format("{0}", GameFlow.Instance.MaxDepth);
			}
			if (scoreText != null)
			{
				scoreText.text = string.Format("{0}", GameFlow.Instance.Score);
			}
			if (highScoreText != null)
			{
				highScoreText.text = string.Format("{0}", GameFlow.Instance.Score);
			}
			if (numDeathsText != null)
			{
				numDeathsText.text = string.Format("{0}", GameFlow.Instance.NumDeaths);
			}
	
			var gamepad = Gamepad.current;
			if (gamepad.startButton.wasPressedThisFrame)
			{
				GameFlow.Instance.ResetToGameStart();
				SceneManager.LoadScene("DetectionTestGym");
			}
		}
	}
}
