using System;
using System.Collections;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
	public static CombatManager Instance;

	[field: SerializeField] public Player Player {  get; private set; }
	[SerializeField] private Encounter[] m_encounters;


	// Events
	public Action<Encounter> OnEncounterBegin;
	public Action<string> OnAttackPerformed;
	public Action OnPlayerPhaseEnd;
	public Action<Encounter> OnEncounterEnd;

	// System
	private int m_encounterIndex;
	public Encounter m_currentEncounter { get; private set; }

	private bool m_isPlayerPhase;
	private int m_enemiesProcessed;// Used for enemy attack phase


	#region Initialization Methods

	private void Awake()
	{
		if (Instance == null)
			Instance = this;
		else
			Destroy(this);
	}

	private void Start()
	{
		m_encounterIndex = -1;
	}

	#endregion

	#region Encounter Management

	private void BeginEncounter(int encounterID)
	{
		Player.SetIsGuarding(false);

		m_currentEncounter = m_encounters[encounterID];
		OnEncounterBegin?.Invoke(m_currentEncounter);

		StartPlayerTurn();
	}

	private void DefeatedEncounter()
	{
		OnEncounterEnd?.Invoke(m_currentEncounter);
		m_currentEncounter = null;
	}

	public void AdvanceEncouner()
	{
		m_encounterIndex++;

		if (m_encounterIndex < m_encounters.Length)
			BeginEncounter(m_encounterIndex);
	}

	#endregion

	#region Battle Management

	public void StartPlayerTurn()
	{
		Debug.Log("Player turn start");

		m_isPlayerPhase = true;
		UIManager.Instance.DisplayPlayerOptions(true);
	}

	public void EndPlayerTurn()
	{
		Debug.Log("Player turn end");

		m_isPlayerPhase = false;
		m_enemiesProcessed = 0;
		ProcessEntityTurn();
	}

	public void ProcessEntityTurn()
	{
		if (m_isPlayerPhase)
			return;

		// Check if all enemies are dead
		bool isEncounterDefeated = true;
		foreach (Enemy i in m_currentEncounter.Enemies)
		{
			if (!i.IsDead)
			{
				isEncounterDefeated = false;
				break;
			}
		}

		if (isEncounterDefeated)
		{
			DefeatedEncounter();
			return;
		}


		// If all enemies acted, return to player phase
		if (m_enemiesProcessed >= m_currentEncounter.Enemies.Length)
		{
			StartPlayerTurn();
			return;
		}

		Enemy enemy = m_currentEncounter.Enemies[m_enemiesProcessed];

		if (enemy.BaseStats.IsBoss)
		{
			UIManager.Instance.Won();
		}

		if (enemy.IsDead)
		{
			m_enemiesProcessed++;
			ProcessEntityTurn();
			return;
		}

		// Attack player
		int rand = UnityEngine.Random.Range(0, enemy.BaseAttacks.Length);
		AttackEntity(enemy, Player, enemy.BaseAttacks[rand]);
	}

	private IEnumerator PlayAttackAnim(Entity instigator, Entity victim, SO_Attack attackData, bool isAttackBlocked, bool isAttackSucsesful)
	{
		UIManager.Instance.AddStringToTextQueue($"{instigator.name} used {attackData.AttackName} on {victim.name}!");

		if (!isAttackBlocked)
			UIManager.Instance.AddStringToTextQueue($"It dealt {attackData.Damage} damage!");
		else 
		{
			if (!isAttackSucsesful)
			{
				UIManager.Instance.AddStringToTextQueue($"{victim.name} blocked {instigator.name}'s {attackData.AttackName}!");
				UIManager.Instance.AddStringToTextQueue($"{victim.name} took 0 damage!");
			}
			else
			{
				UIManager.Instance.AddStringToTextQueue($"{victim.name}'s guard was broken!");
				UIManager.Instance.AddStringToTextQueue($"{instigator.name}'s {attackData.AttackName} dealt {attackData.Damage} damage to {victim.name}!");
			}
		}

		// Notify adrenaline drain
		if(instigator is Player player && attackData.AdrenalineCost > 0)
			UIManager.Instance.AddStringToTextQueue($"{instigator.name} consumed {attackData.AdrenalineCost} adrenaline.");

		UIManager.Instance.PlayTextQueue();

		// VFX
		yield return new WaitForSeconds(attackData.AttackDuration);

		// Deal damage
		if (isAttackSucsesful)
		{
			victim.RemoveHealth(attackData.Damage, out bool isDead);
			if (isDead)
			{
				UIManager.Instance.AddStringToTextQueue($"{victim.name} was slain!");
				UIManager.Instance.PlayTextQueue();
			}
			victim.SetIsGuarding(false);
		}

		// Wait for text to finish (UIManager handles it)
		yield return new WaitUntil(() => !UIManager.Instance.IsPrintingTextQueue);

		// Delay
		yield return new WaitForSeconds(.5f);

		if (!m_isPlayerPhase)
		{
			m_enemiesProcessed++;
			ProcessEntityTurn();
		}
		else
		{
			EndPlayerTurn();
		}

	}

	private IEnumerator PlayGuardAnim(Entity instigator)
	{
		UIManager.Instance.AddStringToTextQueue($"{instigator.name} raised their guard!");
		UIManager.Instance.PlayTextQueue();

		// Wait for text to finish (UIManager handles it)
		yield return new WaitUntil(() => !UIManager.Instance.IsPrintingTextQueue);

		// Delay
		yield return new WaitForSeconds(.5f);

		if (!m_isPlayerPhase)
		{
			m_enemiesProcessed++;
			ProcessEntityTurn();
		}
		else
		{
			EndPlayerTurn();
		}

	}

	public void RechargeAdrenaline()
	{
		UIManager.Instance.AddStringToTextQueue($"You missed the bullet... your adrenaline has been restored.");
		Player.SetAdrenaline(Player.MaxAdrenaline);

		EndPlayerTurn();
	}

	#endregion

	#region Damage

	public void AttackEntity(Entity instigator, Entity victim, SO_Attack attackData)
	{
		UIManager.Instance.HideTextPanel();

		bool isGuarding = victim.IsGuarding;

		// Remove adrenaline
		if (instigator is Player player)
			player.RemoveAdrenaline(attackData.AdrenalineCost);

		// Break through guard if prev turn was guarded
		bool isAttackSuccsesful = true;
		if(isGuarding && !victim.WasPrevTurnGuarded)
			isAttackSuccsesful = false;

		// Reset guard
		if (isGuarding)
			victim.SetIsGuarding(false);

		// Start attack anim
		StartCoroutine(PlayAttackAnim(instigator, victim, attackData, isGuarding, isAttackSuccsesful));
	}

	public void EntityGuard(Entity entity)
	{
		UIManager.Instance.HideTextPanel();

		entity.SetIsGuarding(true);

		// Start attack anim
		StartCoroutine(PlayGuardAnim(entity));
	}

	#endregion
}
