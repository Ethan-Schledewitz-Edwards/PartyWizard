using UnityEngine;

public class Encounter : MonoBehaviour
{
	[SerializeField] private Enemy[] m_enemies;

	[SerializeField] public EncounterTrigger EncounterTrigger;

	private void Awake()
	{
		EncounterTrigger = GetComponentInChildren<EncounterTrigger>(true);

		if (EncounterTrigger == null)
		{
			Debug.LogWarning("An encounter is missing a encounter trigger");
		}
	}
}
