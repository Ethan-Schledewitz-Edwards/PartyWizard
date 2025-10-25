using UnityEngine;

public class Encounter : MonoBehaviour
{
	[field: SerializeField] public Enemy[] Enemies;

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
