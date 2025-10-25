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
	private const float k_brakeForce = 5f;
	private const float k_stopThreshold = 0.5f;

	// Rails
	[SerializeField] private SplineContainer m_railNetwork;
	private Spline m_currentSpline;

	private Rigidbody m_rb;

	// System
	private Vector3 m_velocity;
	private bool m_isAccelerating = false;
	private bool m_isBraking = false;
	private bool m_isStopped = false;

	private void Awake()
	{
		m_rb = GetComponent<Rigidbody>();
		m_rb.useGravity = false;
		m_rb.isKinematic = true;

		m_currentSpline = m_railNetwork != null ? m_railNetwork.Splines[0] : null;
	}

	protected override void Start()
	{
		base.Start();

		CombatManager.Instance.OnEncounterEnd += StartMovement;
		StartMovement(null);
	}

	#region Adrenaline

	public void SetAdrenaline(int value)
	{
		Adrenaline = Mathf.Clamp(value, 0, MaxAdrenaline);
	}

	public void RemoveAdrenaline(int value) => SetAdrenaline(Adrenaline - value);
	#endregion

	private void FixedUpdate()
	{
		if (!m_isStopped)
		{
			if (m_isAccelerating && !m_isBraking)
			{
				Accelerate(k_acceleration);
			}
		}

		if (m_currentSpline == null)
			return;

		SnapToRail();
	}

	#region Physics

	public void StartMovement(Encounter encounter)
	{
		m_isAccelerating = true;
		m_isBraking = false;
		m_isStopped = false;
	}

	public void StopMovement()
	{
		m_isAccelerating = false;
		m_isBraking = true;
	}

	private void Accelerate(float power)
	{
		Vector3 newVel = m_velocity + (transform.forward * power * Time.deltaTime);
		SetVelocity(newVel);
	}

	public void SnapToRail()
	{
		if (m_currentSpline == null) return;
		NativeSpline spline = new NativeSpline(m_currentSpline);

		// Update Pos
		SplineUtility.GetNearestPoint(spline, transform.position, out float3 nearest, out float t);
		Vector3 tangentDir = Vector3.Normalize(spline.EvaluateTangent(t));
		Vector3 up = spline.EvaluateUpVector(t);

		// Update Rotation
		Quaternion targetRot = Quaternion.LookRotation(tangentDir, up);
		Quaternion smoothRot = Quaternion.Slerp(transform.rotation, targetRot, k_rotSpeed * Time.fixedDeltaTime);
		m_rb.MoveRotation(smoothRot);

		// Set vel
		Vector3 velocityAlongSpline = tangentDir * m_velocity.magnitude;
		SetVelocity(velocityAlongSpline);

		m_rb.MovePosition((Vector3)nearest + m_velocity * Time.fixedDeltaTime);

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
	}
	#endregion
}
