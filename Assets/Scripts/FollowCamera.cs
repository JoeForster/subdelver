using TMPro.Examples;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
	public Camera followingCamera;
	public Transform followTarget;
	public Vector3 followOffset = new Vector3(0, 0, -5);
	public float followMoveSensitivity = 8.0f;
	public float followTurnSensitivity = 8.0f;
	public float avoidanceRadius = 2.0f;
	public bool lookAtTargetWhenBlocked = true;

	private bool _hasLastHit = false;
	private Vector3 _lastHitPosition;
	private Vector3 _lastTargetPosition;
	private Vector3 _lastDesiredCameraPosition;

	void FixedUpdate()
    {
		if (followingCamera == null || followTarget == null)
			return;

		// Really simple camera logic with some smoothing.
		// TODO: Probably need to implement stuff like max speed, max distance, etc.
		Vector3 currentTargetPosition = followTarget.position;
		Vector3 desiredCameraPosition = followTarget.position + followTarget.rotation * followOffset;
		Vector3 prevCameraPosition = followingCamera.transform.position;

		// Check for obstructions between the camera target and the camera's next position
		bool gotHit = false;
		_hasLastHit = false;
		_lastDesiredCameraPosition = desiredCameraPosition;
		_lastTargetPosition = currentTargetPosition;
        Vector3 targetToDesiredCameraPos = desiredCameraPosition - currentTargetPosition;
		if (targetToDesiredCameraPos.magnitude > float.Epsilon)
		{
			RaycastHit hit;
			gotHit = Physics.SphereCast(currentTargetPosition, avoidanceRadius, targetToDesiredCameraPos, out hit, followOffset.magnitude);
			if (gotHit)
			{
				// Instead of the ideal position, move the camera to a position just closer than the hit point so that view is not blocked
				desiredCameraPosition = hit.point;

				_hasLastHit = true;
				_lastHitPosition = hit.point;
			}
		}
		
		Vector3 newCameraPosition = Vector3.Lerp(prevCameraPosition, desiredCameraPosition, followMoveSensitivity * Time.deltaTime);
		followingCamera.transform.position = newCameraPosition;
		
		// For now just turn the camera gradually to match the target's rotation.		
		Quaternion lastRot = followingCamera.transform.rotation;
		Quaternion nextRot = lastRot;
		if (gotHit)
		{
			bool turnTowardsTarget;
			if (lookAtTargetWhenBlocked)
			{
				turnTowardsTarget = true;
			}
			else
			{
				Vector3 targetViewportPoint	= followingCamera.WorldToViewportPoint(currentTargetPosition, Camera.MonoOrStereoscopicEye.Mono);
				bool isInView = (targetViewportPoint.x > 0 && targetViewportPoint.x < 1 &&
								 targetViewportPoint.y > 0 && targetViewportPoint.y < 1 &&
								 targetViewportPoint.z > 0);
				turnTowardsTarget = !isInView;
			}

			if (turnTowardsTarget)
			{
				Vector3 cameraToTarget = currentTargetPosition - newCameraPosition;
				nextRot = Quaternion.Slerp(lastRot, Quaternion.LookRotation(cameraToTarget, followTarget.transform.up), followTurnSensitivity * Time.deltaTime);
			}
			else
			{
				nextRot = followingCamera.transform.rotation;
			}
		}
		else
		{
			Quaternion targetRot = followTarget.rotation;
			nextRot = Quaternion.Slerp(lastRot, targetRot, followTurnSensitivity * Time.deltaTime);
		}

		followingCamera.transform.rotation = nextRot;
    }

	
	void OnDrawGizmos()
    {
		if (_hasLastHit)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(_lastHitPosition, avoidanceRadius);
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(_lastDesiredCameraPosition, avoidanceRadius);
		}
		else
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(_lastDesiredCameraPosition, avoidanceRadius);
		}
		Gizmos.DrawLine(_lastTargetPosition, _lastDesiredCameraPosition);

		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(_lastTargetPosition, avoidanceRadius);
    }
}
