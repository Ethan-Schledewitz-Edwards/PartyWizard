using System;
using System.Collections;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
	public static CombatManager Instance;

	[field: SerializeField] public Player Player {  get; private set; }
	[SerializeField] private Encounter[] m_encounters;

	// System
	private int encounterIndex;
	public Encounter currentEncounter;

	// Events
	public Action<Encounter> OnEncounterBegin;
	public Action<string> OnAttackPerformed;
	public Action OnPlayerPhaseEnd;
	public Action<Encounter> OnEncounterEnd;

	private bool isPlayerPhase;
	private bool isPlayingAttackAnimation;
	private int enemiesProcessed;// Used for enemy attack phase

	private void Awake()
	{
		if (Instance == null)
			Instance = this;
		else
			Destroy(this);
	}

	private void Start()
	{
		encounterIndex = -1;

		// Subscribe to player death
		if (Player != null)
		{
			Player.OnDeath += EntityDied;
		}

		UIManager.Instance.OnTextFinished += ProcessEntityTurn;
	}

	#region Encounter Management

	private void BeginEncounter(int encounterID)
	{
		currentEncounter = m_encounters[encounterID];
		OnEncounterBegin?.Invoke(currentEncounter);

		StartPlayerTurn();
	}

	private void FinishEncounter()
	{
		OnEncounterEnd?.Invoke(currentEncounter);
		currentEncounter = null;
	}

	public void AdvanceEncouner()
	{
		encounterIndex++;

		if (encounterIndex < m_encounters.Length)
			BeginEncounter(encounterIndex);
	}

	#endregion

	#region Battle Management

	public void StartPlayerTurn()
	{
		isPlayerPhase = true;
		Debug.Log("Player turn start");
		UIManager.Instance.DisplayPlayerOptions(true);
	}

	public void EndPlayerTurn()
	{
		isPlayerPhase = false;
		enemiesProcessed = 0;
		ProcessEntityTurn();
	}

	public void ProcessEntityTurn()
	{
		if (isPlayerPhase)
			return;

		// If all enemies acted, return to player phase
		if (enemiesProcessed >= currentEncounter.Enemies.Length)
		{
			StartPlayerTurn();
			return;
		}

		Enemy enemy = currentEncounter.Enemies[enemiesProcessed];

		if (enemy.IsDead)
		{
			enemiesProcessed++;
			ProcessEntityTurn();
			return;
		}

		int rand = UnityEngine.Random.Range(0, enemy.BaseAttacks.Length);
		AttackEntity(enemy, Player, enemy.BaseAttacks[rand]);
	}

	private IEnumerator PlayAttackAnim(Entity instigator, Entity victim, SO_Attack attackData)
	{
		// VFX
		yield return new WaitForSeconds(0.5f);

		// Print text
		string text = $"{instigator.name} used {attackData.AttackName} on {victim.name}!";

		if (attackData.Damage > 0)
			text += $" It dealt {attackData.Damage} damage!";

		OnAttackPerformed?.Invoke(text);

		// Apply damage


		// Wait for text to finish (UIManager handles it)
		yield return new WaitUntil(() => UIManager.Instance.IsTextPrinting);

		if (!isPlayerPhase)
		{
			enemiesProcessed++;
			ProcessEntityTurn();
		}
		else
		{
			EndPlayerTurn();
		}
	}

	#endregion

	#region Damage

	public void AttackEntity(Entity instigator, Entity victim, SO_Attack attackData)
	{
		StartCoroutine(PlayAttackAnim(instigator, victim, attackData));
	}

	#endregion

	public void EntityDied(Entity entity)
	{
		if(entity == null)
			return;

		if(entity is Player player)
		{
			// Game over
		}
		else if (entity is Enemy enemy)
		{
			// Check if all enemies are dead
			bool isEncounterDefeated = true;
			foreach (Enemy i in currentEncounter.Enemies)
			{
				if(!i.IsDead)
				{
					isEncounterDefeated = false;
					break;
				}
			}

			if (isEncounterDefeated)
				FinishEncounter();
		}

	}
}
