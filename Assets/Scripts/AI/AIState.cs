using static MonsterController;
using UnityEngine;

enum MonsterControllerState
{
	Idle,
	PatrolMove,
	ChaseTarget,
	SearchTarget
}

abstract class AIState
{
	public AIState(MonsterController parent)
	{
		this.parent = parent;
	}

	// State machine interface
	public virtual void OnEnter() {}
	public abstract MonsterControllerState Update();
	public virtual void OnExit() {}

	// Protected access to parent knowledge and state
	protected AIKnowledge knowledge { get { return parent.knowledge; } }
	protected Transform transform { get { return parent.transform; } }
	// TODO move these into a config scriptable object
	protected MonsterType monsterType { get { return parent.monsterType; } }
	protected float moveSpeed_patrol { get { return parent.moveSpeed_patrol; } }
	protected float moveSpeed_chase { get { return parent.moveSpeed_chase; } }
	protected float arriveDist { get { return parent.arriveDist; } }
	protected float searchTimeLimitSecs { get { return parent.searchTimeLimitSecs; } }


	private MonsterController parent;

	public Patrol FindBestPatrol()
	{
		// This is inefficient and may not scale, but should be OK for now.
		Patrol closestPatrol = null;
		float closestDistSq = -1;
		foreach (Patrol patrol in GameObject.FindObjectsOfType<Patrol>())
		{
			if (!patrol.HasMonster(monsterType))
			{
				continue;
			}

			float thisDistSq = (patrol.transform.position - transform.position).sqrMagnitude;
			if (closestDistSq < 0 || thisDistSq < closestDistSq)
			{
				closestPatrol = patrol;
				closestDistSq = thisDistSq;
			}
		}
		return closestPatrol;
	}

	public bool FindNextPatrolPoint()
	{
		if (knowledge._currentPatrol == null || knowledge._currentPatrol.patrolPoints.Length == 0)
		{
			return false;
		}
		else
		{
			Vector3 curPos = transform.position;
			Transform[] points = knowledge._currentPatrol.patrolPoints;

			// First patrol point - find closest.
			if (knowledge._currentPatrolPointIndex < 0)
			{
				//Transform closestPoint;
				int closestPointIndex = -1;
				float closestPointDistSq = -1;
				for (int i = 0; i < points.Length; i++)
				{
					float thisPointDistSq = (curPos - points[i].position).sqrMagnitude;
					if (closestPointIndex < 0 || thisPointDistSq < closestPointDistSq)
					{
						closestPointIndex = i;
						closestPointDistSq = thisPointDistSq;
					}
				}
				Debug.Assert(closestPointIndex >= 0);
				knowledge._currentPatrolPointIndex = closestPointIndex;
			}

			knowledge._currentPatrolPointIndex = knowledge._currentPatrolPointIndex + 1;
			if (knowledge._currentPatrolPointIndex >= points.Length)
			{
				knowledge._currentPatrolPointIndex = 0;
			}

			knowledge._patrolDestination = points[knowledge._currentPatrolPointIndex].position;
			return true;
		}
	}

	// TEMP LOCATION not best place for this!
	protected void UpdateMoveToTarget(Vector3 target, float maxSpeed, float arriveDistance, out bool arrived)
	{
		Vector3 meToDest = target - transform.position;

		if (meToDest.magnitude < arriveDistance)
		{
			arrived = true;
		}
		else
		{
			arrived = false;

			// Kinematic on-rails monster movement for now.

			// Move direct to destination
			// TODO move in face direction
			transform.position += meToDest.normalized * maxSpeed * Time.deltaTime;
			
			// Gradually rotate towards destination
			Quaternion desiredRot = Quaternion.LookRotation(meToDest);
			transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, parent.turnRate * Time.deltaTime);
		}
	}
};