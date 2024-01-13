using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orientor : MonoBehaviour
{
	public Transform player;
	public Vector3 offset;

	private void FixedUpdate()
	{
		if (transform != null)
		{
			transform.position = player.position + offset;
		}
	}
}
