using System;
using System.Text;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
	public static CombatManager Instance;

	[SerializeField] private Player m_player;
	[SerializeField] private Encounter[] m_encounters;

	// System
	private int encounterIndex;
	private Encounter currentEncounter;

	// Events
	public Action<Encounter> OnEncounterBegin;
	public Action<Encounter> OnEncounterEnd;

	private void Awake()
	{
		if (Instance == null)
			Instance = this;
		else
			Destroy(this);
	}

	private void Start()
	{
		encounterIndex = 0;
		BeginEncounter(encounterIndex);
	}

	private void BeginEncounter(int encounterID)
	{
		Encounter encouner = m_encounters[encounterID];

		//m_player.transform.position = encouner.PlayerPos.position;

		OnEncounterBegin?.Invoke(encouner);
	}

	private void FinishEncounter()
	{
		// Move player

		OnEncounterBegin?.Invoke(currentEncounter);
		currentEncounter = null;
	}

	public void AdvanceEncouner()
	{
		encounterIndex++;
		BeginEncounter(encounterIndex);
	}
}
