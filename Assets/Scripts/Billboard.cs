using UnityEngine;

public class Billboard : MonoBehaviour
{
	private Camera m_mainCamera;
	private float m_rotationSpeed = 100f;

	void Start()
	{
		m_mainCamera = Camera.main;
	}

	void Update()
	{
		Quaternion targetRot = Quaternion.LookRotation(m_mainCamera.transform.position - transform.position);
		targetRot.x = 0;
		targetRot.z = 0;
		transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, m_rotationSpeed * Time.deltaTime);
	}
}
