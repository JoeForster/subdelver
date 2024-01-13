using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public int ScoreValue = 10;
	public int OxygenValue = 120;
	public int DecoyValue = 2;

	private void OnTriggerEnter(Collider other)
	{
		GameObject hitObject = other.gameObject;
		// TODO decouple from controls, should be a "player" object?
		SubmarineControls player = hitObject.GetComponent<SubmarineControls>();
		if (player != null)
		{
			GameFlow.Instance.Score += ScoreValue;
			GameFlow.Instance.NumDecoys += DecoyValue;
			GameFlow.Instance.Oxygen += OxygenValue;
			Destroy(gameObject);
		}
	}
}
