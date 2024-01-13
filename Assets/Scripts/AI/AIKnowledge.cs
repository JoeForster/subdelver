using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIKnowledge
{
	// State: PatrolMove
	public Patrol _currentPatrol;
	public Vector3 _patrolDestination = Vector3.zero;
	public int _currentPatrolPointIndex = -1;
	// State: Common
	public SortedSet<Detectable> _detectedObjectsByPriority = new SortedSet<Detectable>(new CompareByPriority());
	// State: SearchTarget
	public Vector3 _searchLastKnownPos;
	public bool _searchHasLastKnownPos = false;
	public float _searchTimeLeftSecs;

	public Detectable GetBestDetectable()
	{
		if (_detectedObjectsByPriority.Count == 0)
		{
			return null;
		}
		else
		{
			return _detectedObjectsByPriority.ElementAt(0);
		}
	}
};