using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patrol : MonoBehaviour
{
    public Transform[] patrolPoints;

	// TODO figure out how to do this as a bitfield with a nice checkbox or multiselect UI instead.
	public List<MonsterController.MonsterType> monsterTypes;

	public void OnDrawGizmos()
	{
		if (patrolPoints != null)
		{
			Gizmos.color = Color.white;
			foreach (Transform t in patrolPoints)
			{
				Gizmos.DrawSphere(t.position, 0.5f);
			}
		}
	}

	public bool HasMonster(MonsterController.MonsterType monsterType)
	{
		return monsterTypes.Contains(monsterType);
	}
}
