using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

// TODO getting a bit big, needs breaking up
public class MonsterController : MonoBehaviour
{
	public enum MonsterType
	{
		Anglerfish,
		Shark,
		Jellyfish
	}

	public bool HasArrivedAtDestination()
	{
		if (navAgent != null)
		{
			if (!navAgent.pathPending)
			{
				return  (!navAgent.hasPath || (navAgent.remainingDistance <= navAgent.stoppingDistance && navAgent.velocity.sqrMagnitude == 0f));
			}
			return false;
		}
		else
		{
			return arrivedAtDestination;
		}
	}


	public SphereCollider detector;
	public Detectable.Type detectType;
	public MonsterType monsterType;
	public float moveSpeed_patrol = 2.0f;
	public float moveSpeed_chase = 4.0f;
	public float turnRate = 3.0f;
	public float searchTimeLimitSecs = 4.0f;

	public AIKnowledge knowledge { get { return _knowledge; } }
	
	private AIKnowledge _knowledge;
	private MonsterControllerState _currentState = MonsterControllerState.Idle;

	private Dictionary<MonsterControllerState, AIState> _states = new Dictionary<MonsterControllerState, AIState>();
	
	private NavMeshAgent navAgent;

	// Properties just for non-navagent moving, i.e. just moving direct to destinations with no pathfinding.
	// TODO either deprecate/remove once we have full volume pathfinding, or move into our own nav agent component?
	public float arriveDist = 0.1f;

	private float moveSpeed;
	private Vector3 moveDestination;
	private bool hasDestination = false;
	private bool arrivedAtDestination = true;

	public void SetNavDestination(Vector3 newTarget)
	{
		if (navAgent != null)
		{
			navAgent.SetDestination(newTarget);
		}
		moveDestination = newTarget;
		hasDestination = true;
		arrivedAtDestination = false;
	}

	public void SetMoveSpeed(float newMoveSpeed)
	{
		if (navAgent != null)
		{
			navAgent.speed = newMoveSpeed;
		}
		moveSpeed = newMoveSpeed;
	}

	void Start()
	{
		// Init knowledge
		_knowledge = new AIKnowledge();

		// Init AI state machine
		_states[MonsterControllerState.Idle] = new MonsterIdleState(this);
		_states[MonsterControllerState.PatrolMove] = new MonsterPatrolMoveState(this);
		_states[MonsterControllerState.ChaseTarget] = new MonsterChaseTargetState(this);
		_states[MonsterControllerState.SearchTarget] = new MonsterSearchTargetState(this);

		// Unity navigation agent - optional, only used for gym testing at the moment
		navAgent = GetComponent<NavMeshAgent>();
	}
		
	// Simple boilerplate-heavy state machine - will probably end up being split off into own class
    void Update()
    {
		// DETECTION LOGIC update
		for (int i = knowledge._detectedObjectsByPriority.Count-1; i >= 0; --i)
		{
			Detectable d = knowledge._detectedObjectsByPriority.ElementAt(i);
			if (d == null || d.gameObject == null || !d.IsEnabled())
			{
				knowledge._detectedObjectsByPriority.Remove(d);
			}
		}

		// AI LOGIC state machine update
		MonsterControllerState prevState = _currentState;
		MonsterControllerState newState = prevState;
		AIState stateObj = _states[_currentState];
		newState = stateObj.Update();
		if (newState != prevState)
		{
			stateObj.OnExit();
			_currentState = newState;
			_states[newState].OnEnter();
		}

		UpdateMovement();
    }

	private void UpdateMovement()//Vector3 target, float maxSpeed, float arriveDistance, out bool arrived)
	{
		// Skip if we have a nav mesh agent governing movement.
		if (navAgent != null)
		{
			return;
		}

		if (!hasDestination)
		{
			arrivedAtDestination = true;
			return;
		}


		Vector3 meToDest = moveDestination - transform.position;
	
		if (meToDest.magnitude < arriveDist)
		{
			arrivedAtDestination = true;
		}
		else
		{
			arrivedAtDestination = false;
	
			// Kinematic on-rails monster movement for now.
			// TODO more natural movement along curved paths
			// (on-rails to begin with, then proper thrust-based physics control for more natural chasing/manoeuvring)
	
			// Move direct to destination
			// TODO move in face direction
			transform.position += meToDest.normalized * moveSpeed * Time.deltaTime;
			
			// Gradually rotate towards destination
			Quaternion desiredRot = Quaternion.LookRotation(meToDest);
			transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, turnRate * Time.deltaTime);
		}
	}

	void OnTriggerEnter(Collider other)
	{
		// TODO would be better to use the collision layers rather than manually filtering here?
		Detectable detected = other.gameObject.GetComponent<Detectable>();
		if (detected != null)
		{
			if (detected.detectType != Detectable.Type.None &&
				detected.detectType == detectType && 
				detected.IsEnabled())
			{
				knowledge._detectedObjectsByPriority.Add(detected);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		Detectable detected = other.gameObject.GetComponent<Detectable>();
		if (detected != null)
		{
			knowledge._detectedObjectsByPriority.Remove(detected);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		GameObject hitObject = collision.collider.gameObject;
		Destructible destructible = hitObject.GetComponent<Destructible>();
		if (destructible != null)
		{
			destructible.hitPoints -= 1;
			if (destructible.hitPoints <= 0)
			{
				Destroy(hitObject);
			}
		}
	}

	void OnDrawGizmos()
	{
        switch (_currentState)
		{
			case MonsterControllerState.Idle:
				Gizmos.color = Color.grey;
				Gizmos.DrawSphere(transform.position, 1.0f);
				break;
			case MonsterControllerState.PatrolMove:
				Gizmos.color = Color.yellow;
				Gizmos.DrawLine(transform.position, knowledge._patrolDestination);
				break;
		}

		if (knowledge == null)
		{
			return;
		}

		Detectable best = knowledge.GetBestDetectable();
		if (best != null)
		{
			foreach (Detectable detected in knowledge._detectedObjectsByPriority)
			{
				Gizmos.color = (detected == best) ? Color.red : Color.yellow;
				Gizmos.DrawWireSphere(detected.transform.position, 0.5f);
			}
		}
		else
		{
			if (knowledge._searchHasLastKnownPos)
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawWireSphere(knowledge._searchLastKnownPos, 0.5f);
			}
		}

	}
}
