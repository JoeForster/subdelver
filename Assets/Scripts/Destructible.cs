using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Destructible : MonoBehaviour
{
	public int hitPoints = 1;
	public float lifeTime = -1.0f;


	private void Update()
	{
		if (lifeTime >= 0.0f)
		{
			lifeTime -= Time.deltaTime;
			if (lifeTime <= 0.0f)
			{
				Destroy(gameObject);
			}
		}
	}
}
