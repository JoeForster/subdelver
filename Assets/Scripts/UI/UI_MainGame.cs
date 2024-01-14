using System.Collections;
using System.Collections.Generic;
using TMPro;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(GameFlow_MainGame))]
public class UI_MainGame : MonoBehaviour
{
	private GameFlow_MainGame inGameFlow;

	public TMP_Text depthLabel;
	public TMP_Text scoreLabel;
	public TMP_Text decoysLabel;
	public TMP_Text oxygenLabel;
	public TMP_Text escapeLabel;
	public TMP_Text noOxygenLabel;

	public void Start()
	{
		Debug.Assert(GameFlow_Global.Instance != null, "MISSING GameFlow component, should be on Globals!");
        inGameFlow = GetComponent<GameFlow_MainGame>();
	}


	void Update()
    {
		bool isAtSurface = inGameFlow.runState.CurDepth <= 0;

		// UI update
		if (GameFlow_Global.Instance != null)
		{
			if (depthLabel != null)
			{
				depthLabel.text = string.Format("Depth: {0} (Max: {1})", inGameFlow.runState.CurDepth, inGameFlow.runState.MaxDepth);
			}
			if (scoreLabel != null)
			{
				scoreLabel.text = string.Format("Score: {0}", inGameFlow.runState.Score);
			}
			if (decoysLabel != null)
			{
				decoysLabel.text = string.Format("Decoys Left: {0}", inGameFlow.runState.NumDecoys);
			}
			if (oxygenLabel != null)
			{
				oxygenLabel.text = string.Format("Oxygen Left: {0}", (int)inGameFlow.runState.Oxygen);
			}
			if (escapeLabel != null)
			{
				escapeLabel.enabled = isAtSurface && inGameFlow.State == GameFlow_MainGame.GameState.InGame;
			}

			if (inGameFlow.State == GameFlow_MainGame.GameState.PlayerDead && inGameFlow.runState.Oxygen <= 0.0f)
			{
				noOxygenLabel.text = "Out of Oxygen...";
			}
		}
    }
}
