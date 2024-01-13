using UnityEngine;

class MonsterIdleState : AIState
{
	public MonsterIdleState(MonsterController parent) : base(parent) {}

	public override MonsterControllerState Update()
	{
		if (knowledge._detectedObjectsByPriority.Count > 0)
		{
			return MonsterControllerState.ChaseTarget;
		}
		else
		{
			if (knowledge._currentPatrol == null)
			{
				knowledge._currentPatrol = FindBestPatrol();
			}

			if (knowledge._currentPatrol != null)
			{
				return MonsterControllerState.PatrolMove;
			}
		}

		return MonsterControllerState.Idle;
	}
};

class MonsterPatrolMoveState : AIState
{
	public MonsterPatrolMoveState(MonsterController parent) : base(parent) {}

	public override void OnEnter()
	{
		if (!FindNextPatrolPoint())
		{
			Debug.LogWarning("Failed to find patrol point althouth a patrol was set - patrol will be broken!");
		}
	}
	public override MonsterControllerState Update()
	{
		if (knowledge._detectedObjectsByPriority.Count > 0)
		{
			return MonsterControllerState.ChaseTarget;
		}
		else if (knowledge._currentPatrolPointIndex >= 0)
		{
			bool arrived;
			UpdateMoveToTarget(knowledge._patrolDestination, moveSpeed_patrol, arriveDist, out arrived);

			if (arrived)
			{
				// This currently won't normally happen, but could if our patrol went away or if it's a one-way path I guess
				if (!FindNextPatrolPoint())
				{
					Debug.LogWarning("Patrol unexpectedly ended - went to idle!");
					return MonsterControllerState.Idle;
				}
			}
		}
		return MonsterControllerState.PatrolMove;
	}
	public override void OnExit()
	{
		knowledge._currentPatrol = null;
		knowledge._currentPatrolPointIndex = -1;
	}
}

class MonsterChaseTargetState : AIState
{
	public MonsterChaseTargetState(MonsterController parent) : base(parent) {}

	public override void OnEnter()
	{
	}
	public override MonsterControllerState Update()
	{
		if (knowledge._detectedObjectsByPriority.Count == 0)
		{
			return MonsterControllerState.SearchTarget;
		}
		else
		{
			// NOTE: This doesn't care about which is closest if we detect multiple at the same priority.
			bool arrived;
			Detectable detected = knowledge.GetBestDetectable();
			UpdateMoveToTarget(detected.transform.position, moveSpeed_chase, arriveDist, out arrived);
			knowledge._searchLastKnownPos = detected.transform.position;
			knowledge._searchHasLastKnownPos = true;

			return MonsterControllerState.ChaseTarget;
		}
	}
	public override void OnExit()
	{
	}
}

class MonsterSearchTargetState : AIState
{
	public MonsterSearchTargetState(MonsterController parent) : base(parent) {}

	public override void OnEnter()
	{
		knowledge._searchTimeLeftSecs = searchTimeLimitSecs;
	}
	public override MonsterControllerState Update()
	{
		if (knowledge._detectedObjectsByPriority.Count > 0)
		{
			return MonsterControllerState.ChaseTarget;
		}
		else if (!knowledge._searchHasLastKnownPos)
		{
			return MonsterControllerState.Idle;
		}
		else
		{
			bool arrived;
			UpdateMoveToTarget(knowledge._searchLastKnownPos, moveSpeed_chase, arriveDist, out arrived);
			if (arrived)
			{
				knowledge._searchTimeLeftSecs -= Time.deltaTime;
				if (knowledge._searchTimeLeftSecs <= 0.0f)
				{
					return MonsterControllerState.Idle;
				}
			}

			return MonsterControllerState.SearchTarget;
		}
	}
	public override void OnExit()
	{
		knowledge._searchHasLastKnownPos = false;
	}
}
