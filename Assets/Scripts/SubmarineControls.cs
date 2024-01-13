using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering.HighDefinition;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class SubmarineControls : MonoBehaviour
{
	public float thrustDeadZone = 0.1f;
	public float mainThrustScale = 5.0f;
	public float lateralThrustScale = 2.0f;
	public float turnThrustScale = 2.0f;
	public float MaxSpeed;

	public Rigidbody rigidBody;

	public ParticleSystem trailParticles;
	public GameObject decoyPrefab;
	public Transform decoyShootForwardsPos;
	public float decoyShootForwardsForce = 5.0f;
	public Transform decoyShootBackwardsPos;
	public float decoyShootBackwardsForce = 1.0f;

	public Detectable engineSoundDetectable;
	public float mainThrustNoiseScale = 1.0f;
	public float lateralThrustNoiseScale = 0.5f;
	public float turnThrustNoiseScale = 0.5f;

	public Detectable torchLightDetectable;

	private Vector3 _inputThrust;
	private Vector3 _inputYawPitchRoll;
	private Quaternion _thrustRotation;
	private float _engineThrustNoiseLevel = 0.0f;
	private float _trailParticlesMaxRate = -1.0f;

	void Start()
	{
		_inputThrust = Vector3.zero;
		_inputYawPitchRoll = Vector3.zero;
		_thrustRotation = Quaternion.identity;
        Debug.Log(string.Join("\n", Gamepad.all));

		Debug.Assert(rigidBody != null);
		if (rigidBody != null)
		{
			rigidBody.maxLinearVelocity = MaxSpeed;
		}
	}

	// Per-frame update for reading inputs and processing control logic
	void Update()
	{
		var gamepad = Gamepad.current;
		if (gamepad != null)
		{
			Update_Thrust(gamepad);
			Update_Shoot(gamepad);
			Update_Light(gamepad);
		}
		
		Update_Particles();
	}

	void Update_Thrust(Gamepad gamepad)
	{
		// Y is fixed as up relative to our orientation
		_engineThrustNoiseLevel = 0.0f;
		{
			Vector2 moveLateral = gamepad.leftStick.ReadValue();
			float lateralThrustLevel = moveLateral.magnitude;
			if (lateralThrustLevel > thrustDeadZone)
			{
				Vector2 thrustLateral = moveLateral * lateralThrustScale;
				_inputThrust.x = thrustLateral.x;
				_inputThrust.y = thrustLateral.y;
				
				_engineThrustNoiseLevel = Mathf.Max(_engineThrustNoiseLevel, lateralThrustLevel * lateralThrustNoiseScale);
			}
			else
			{
				_inputThrust.x = 0.0f;
				_inputThrust.y = 0.0f;
			}
		}		
		{
			float mainThrust = Mathf.Min(1.0f, gamepad.rightTrigger.ReadValue() - gamepad.leftTrigger.ReadValue());
			if (Mathf.Abs(mainThrust) > thrustDeadZone)
			{
				_inputThrust.z = mainThrust * mainThrustScale;
				
				_engineThrustNoiseLevel = Mathf.Max(_engineThrustNoiseLevel, mainThrust * mainThrustNoiseScale);
			}
			else
			{
				_inputThrust.z = 0.0f;
			}
		}
		
		{
			Vector2 moveYawPitch = gamepad.rightStick.ReadValue();
			float moveYawPitchMag = moveYawPitch.magnitude;
			if (moveYawPitchMag > thrustDeadZone)
			{
				Vector2 thrust2D = moveYawPitch.normalized * turnThrustScale;
				_inputYawPitchRoll.Set(thrust2D.x, thrust2D.y, 0);
				
				_engineThrustNoiseLevel = Mathf.Max(_engineThrustNoiseLevel, moveYawPitchMag * turnThrustNoiseScale);
			}
			else
			{
				_inputYawPitchRoll.x = 0;
				_inputYawPitchRoll.y = 0;
			}
		}		
		{
			float moveRoll = Mathf.Min(1.0f, gamepad.leftShoulder.ReadValue() - gamepad.rightShoulder.ReadValue());
			float moveRollAbs = Mathf.Abs(moveRoll);
			if (moveRollAbs > thrustDeadZone)
			{
				_inputYawPitchRoll.z = moveRoll * turnThrustScale;
				
				_engineThrustNoiseLevel = Mathf.Max(_engineThrustNoiseLevel, moveRollAbs * turnThrustNoiseScale);
			}
			else
			{
				_inputYawPitchRoll.z = 0.0f;
			}
		}

		if (engineSoundDetectable != null)
		{
			engineSoundDetectable.SetEnabled(_engineThrustNoiseLevel > 0.0f);
			engineSoundDetectable.transform.localScale = new Vector3(_engineThrustNoiseLevel, _engineThrustNoiseLevel, _engineThrustNoiseLevel);
		}
	}

	void Update_Particles()
	{
		// show bubbles if thrusting
		if (trailParticles != null)
		{
			var emission = trailParticles.emission;

			if (_trailParticlesMaxRate < 0.0f)
			{
				_trailParticlesMaxRate = emission.rateOverTimeMultiplier;
			}

			emission.enabled = (_engineThrustNoiseLevel > 0.0f);
			emission.rateOverTimeMultiplier = _trailParticlesMaxRate * _engineThrustNoiseLevel;
		}
	}

	void Update_Shoot(Gamepad gamepad)
	{
		if (GameFlow.Instance.NumDecoys == 0)
		{
			return;
		}

		if (decoyShootForwardsPos != null && gamepad.aButton.wasPressedThisFrame)
		{
			Vector3 decoySpawnPos = decoyShootForwardsPos.position;
			GameObject spawnedDecoy = Instantiate(decoyPrefab, decoySpawnPos, Quaternion.identity);
			if (spawnedDecoy != null)
			{
				spawnedDecoy.GetComponent<Rigidbody>().AddForce(transform.forward * decoyShootForwardsForce);
				GameFlow.Instance.NumDecoys--;
			}
		}
		else if (decoyShootBackwardsPos != null  && gamepad.bButton.wasPressedThisFrame)
		{
			Vector3 decoySpawnPos = decoyShootBackwardsPos.position;
			GameObject spawnedDecoy = Instantiate(decoyPrefab, decoySpawnPos, Quaternion.identity);
			if (spawnedDecoy != null)
			{
				spawnedDecoy.GetComponent<Rigidbody>().AddForce(-transform.forward * decoyShootBackwardsForce);
				GameFlow.Instance.NumDecoys--;
			}
		}
	}

	void Update_Light(Gamepad gamepad)
	{
		if (torchLightDetectable == null)
		{
			return;
		}

		if (gamepad.xButton.wasPressedThisFrame)
		{
			bool isLightOn = torchLightDetectable.IsEnabled();
			torchLightDetectable.SetEnabled(!isLightOn);
		}
	}

	// Fixed physics update for translating the controls to forces on our vehicle.
	void FixedUpdate()
	{
		if (rigidBody != null)
		{
			rigidBody.AddForce(transform.rotation * _inputThrust);
			rigidBody.AddTorque(transform.rotation * new Vector3(_inputYawPitchRoll.y, _inputYawPitchRoll.x, _inputYawPitchRoll.z));
		}
	}

	void OnDrawGizmos()
    {
		Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.rotation * _inputThrust);
		Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.rotation * _inputYawPitchRoll);
    }
}
