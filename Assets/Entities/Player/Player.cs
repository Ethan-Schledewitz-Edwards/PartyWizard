using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(Rigidbody))]
public class Player : Entity
{
	// Stats
	public int Adrenaline { get; private set; }
	public int MaxAdrenaline { get; private set; } = 10;

	// Movement constants
	private const float k_acceleration = 4f;
	private const float k_maxSpeed = 3f;
	private const float k_rotSpeed = 10f;
	private const float k_brakeForce = 0.25f;
	private const float k_stopThreshold = 0.5f;

	// Rails
	[SerializeField] private SplineContainer m_railNetwork;
	private Spline m_currentSpline;

	private Rigidbody m_rb;

	// System
	private Vector3 m_velocity;
	private bool m_isAccelerating = true;
	private bool m_isBraking = false;
	private bool m_isStopped = false;

	private float m_currentT = 0f;

	private void Awake()
	{
		m_rb = GetComponent<Rigidbody>();
		m_rb.useGravity = false;
		m_rb.isKinematic = true;

		m_currentSpline = m_railNetwork != null ? m_railNetwork.Splines[0] : null;
	}

	public void SetAdrenaline(int value)
	{
		Adrenaline = Mathf.Clamp(value, 0, MaxAdrenaline);
	}

	public void RemoveAdrenaline(int value) => SetAdrenaline(Adrenaline - value);

	private void FixedUpdate()
	{
		if (!m_isStopped)
		{
			if (m_isAccelerating && !m_isBraking)
			{
				Throttle(k_acceleration);
			}
		}

		if (m_currentSpline == null)
			return;

		SnapToRail();
	}

	private void Throttle(float power)
	{
		Vector3 newVel = m_velocity + (transform.forward * power * Time.deltaTime);
		SetVelocity(newVel);
	}

	public void SnapToRail()
	{
		if (m_currentSpline == null) return;

		NativeSpline spline = new NativeSpline(m_currentSpline);

		float currentSpeed = m_velocity.magnitude;
		float splineLength = spline.GetLength();

		float deltaT = currentSpeed * Time.fixedDeltaTime / splineLength;
		m_currentT += deltaT;

		if (m_currentT >= 1f)
		{
			m_currentT = 1f;// Stop at the end
			m_isStopped = true;
			SetVelocity(Vector3.zero);
		}

		// Update Pos
		Vector3 newPosition = (Vector3)spline.EvaluatePosition(m_currentT);
		Vector3 tangentDir = Vector3.Normalize(spline.EvaluateTangent(m_currentT));
		Vector3 up = spline.EvaluateUpVector(m_currentT);
		m_rb.MovePosition(newPosition);

		// Update Rotation
		Quaternion targetRot = Quaternion.LookRotation(tangentDir, up);
		Quaternion smoothRot = Quaternion.Slerp(transform.rotation, targetRot, k_rotSpeed * Time.fixedDeltaTime);
		m_rb.MoveRotation(smoothRot);

		// Set vel
		Vector3 velocityAlongSpline = tangentDir * currentSpeed;
		SetVelocity(velocityAlongSpline);

		// Apply brake force
		if (m_isBraking)
		{
			SetVelocity(Vector3.Lerp(m_velocity, Vector3.zero, k_brakeForce * Time.fixedDeltaTime));

			if (m_velocity.magnitude <= k_stopThreshold)
			{
				m_isStopped = true;
				SetVelocity(Vector3.zero);
			}
		}
		else
		{
			m_isStopped = false;
		}
	}

	public void SetVelocity(Vector3 newVel)
	{
		newVel = Vector3.ClampMagnitude(newVel, m_isStopped ? 0 : k_maxSpeed);
		m_velocity = newVel;
		Debug.Log(m_velocity);
	}
}
