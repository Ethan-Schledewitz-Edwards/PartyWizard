using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class EncounterTrigger : MonoBehaviour
{
	private SphereCollider m_sphereCollider;

    void Start()
    {
		m_sphereCollider = GetComponent<SphereCollider>();
		m_sphereCollider.enabled = true;
		m_sphereCollider.isTrigger = true;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			CombatManager.Instance.AdvanceEncouner();
		}
	}
}
