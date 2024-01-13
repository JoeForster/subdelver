using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A Detectable is an object with detection settings and a Collider component that must be some trigger collider.
// Detectable should be found by MonsterController if the type matches and their detector collider overlaps the Detectable's collider.
[RequireComponent(typeof(Collider))]
public class Detectable : MonoBehaviour
{
	public enum Type
	{
		None = 0, // No detection

		Light, // For monsters attracted to light: detectable itself can be seen when in light or has a light gadget on
		Sound, // For monsters attracted to sound: detectable itself when engine is on
	}

	public int detectPriority = 0; // Monster will select the best (lowest number) priority first before looking at distance.
	public Type detectType = Type.None;
	
	public Collider GetCollider() { return detectionCollider; }
	public bool IsEnabled() { return gameObject.activeSelf; }
	public void SetEnabled(bool isEnabled) { gameObject.SetActive(isEnabled); }

	void Start()
	{
		detectionCollider = GetComponent<Collider>();
		Debug.Assert(detectionCollider != null);
		Debug.Assert(detectionCollider.isTrigger, "Detectable collider must be a trigger!");
	}

	private Collider detectionCollider;
}

public class CompareByPriority : IComparer<Detectable>
{
    public int Compare(Detectable first, Detectable second)
    {
		return first.detectPriority.CompareTo(second.detectPriority);
    }
}