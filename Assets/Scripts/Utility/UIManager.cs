using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	public static UIManager Instance;

	[Header("Canvas")]
	[SerializeField] private GameObject m_canvas;

	[Header("Player Options Panel")]
	[SerializeField] private GameObject m_playerOptions;
	[SerializeField] private SpellButton m_spellButton;
	[SerializeField] private Slider m_healthSlider;
	[SerializeField] private Slider m_adrenalineSlider;

	[Header("Options Panel Effects")]
	[SerializeField] private EnemySelector m_enemySelector;
	[SerializeField] private EnemySelector m_particleEffects;
	[SerializeField] private ParticleSystem m_particleSystem;

	[Header("Wheel screen")]
	[SerializeField] private SpinningWheel m_spinningwheel;

	[Header("Spell screen")]
	[SerializeField] private GameObject m_spellScreen;

	[Header("Text Screen")]
	[SerializeField] private GameObject m_textPanel;
	[SerializeField] private TextMeshProUGUI m_textBox;

	[Header("Game State Screens")]
	[SerializeField] private GameObject m_whiteLightPanel;
	[SerializeField] private GameObject m_gameOverPanel;
	[SerializeField] private GameObject m_playerWonPanel;

	[Header("Audio")]
	[SerializeField] private AudioSource m_sfxSource;
	[SerializeField] private AudioSource m_musicSource;// Bad
	[SerializeField] private AudioClip m_ringing;
	[SerializeField] private AudioClip m_death;
	[SerializeField] private AudioClip m_won;

	private SO_Attack m_currentAttack;
	private InputSystem_Actions m_inputActions;

	// Events
	public Action OnSpellScreenEnd;

	// System
	private Queue<string> m_stringsToType;
	private Enemy[] m_currentEnemies;
	private int m_highlightedEnemy;
	private bool m_isPickingSpell;
	private bool m_shouldHealAfterSpin;

	public bool IsPrintingTextQueue { get; private set; }

	#region Initialization

	private void Awake()
	{
		if (Instance == null)
			Instance = this;
		else
			Destroy(this);

		m_stringsToType = new Queue<string>();
	}

	private void Start()
	{
		CombatManager combatManager = CombatManager.Instance;

		m_canvas.SetActive(false);
		m_playerOptions.SetActive(false);
		m_textPanel.SetActive(false);
		m_spinningwheel.gameObject.SetActive(false);

		m_healthSlider.maxValue = combatManager.Player.MaxHealth;
		m_adrenalineSlider.maxValue = combatManager.Player.MaxAdrenaline;

		combatManager.OnEncounterBegin += BeginEncounter;
		combatManager.OnEncounterEnd += EndEncounter;

		m_spinningwheel.OnBulletHit += FireBullet;
		m_spinningwheel.OnSafe += RechargeAdrenaline;

		m_inputActions = new InputSystem_Actions();

		m_enemySelector.gameObject.SetActive(false);
		m_inputActions.UI.Disable();
        m_inputActions.UI.Navigate.performed += SelectEnemy;
        m_inputActions.UI.Submit.performed += AttackEnemy;
        m_inputActions.UI.Cancel.performed += CancelEnemySelection;
    }
	#endregion

	#region Combat State

	private void BeginEncounter(Encounter encounter)
    {
		m_currentEnemies = encounter.Enemies;
		m_highlightedEnemy = 0;

		m_canvas.SetActive(true);
		DisplayPlayerOptions(true);
	}

	private void EndEncounter(Encounter encounter)
	{
		m_canvas.SetActive(true);
		DisplayPlayerOptions(false);
		HideTextPanel();
		ShowSpellSelectScreen();
	}

	#endregion

	#region Text Queue

	public void AddStringToTextQueue(string text)
	{
		m_stringsToType.Enqueue(text);
	}

	public void PlayTextQueue()
	{
		if (IsPrintingTextQueue || m_stringsToType.Count <= 0)
			return;

		DisplayPlayerOptions(false);
		m_textPanel.SetActive(true);

		StopAllCoroutines();
		IsPrintingTextQueue = true;
		StartCoroutine(ProcessTextQueue());
	}

	private IEnumerator ProcessTextQueue()
	{
		IsPrintingTextQueue = true;

		while (m_stringsToType.Count > 0)
		{
			string nextText = m_stringsToType.Dequeue();
			yield return StartCoroutine(TypeText(nextText));

			// pause between messages
			yield return new WaitForSeconds(0.5f);
		}

		HideTextPanel();
		IsPrintingTextQueue = false;

		// Execute after new spell was rolled and text finished
		if (m_isPickingSpell)
		{
			m_isPickingSpell = false;
			OnSpellScreenEnd?.Invoke();
		}
	}

	private IEnumerator TypeText(string text)
	{
		m_textBox.text = "";

		foreach (char c in text)
		{
			m_textBox.text += c;
			yield return new WaitForSeconds(0.03f);// typing speed
		}

		yield return new WaitForSeconds(0.5f);// delay after message
	}

	public void HideTextPanel()
	{
		IsPrintingTextQueue = false;
		m_textPanel.SetActive(false);
	}
	#endregion

	public void DisplayPlayerOptions(bool isActive)
	{
		m_playerOptions.SetActive(isActive);

		if (isActive)
			ResetPlayerOptions();
	}

	private void ResetPlayerOptions()
	{
		m_spinningwheel.gameObject.SetActive(false);

		// Refresh current spell
		Player player = CombatManager.Instance.Player;
		m_spellButton.SetSpell(player.CurrentSpell);

		// Refresh health and adrenaline bars
		m_healthSlider.value = player.Health;
		m_adrenalineSlider.value = player.Adrenaline;
	}

	#region Buttons

	public void AttackButton()
    {
		CombatManager combatManager = CombatManager.Instance;
		Player player = combatManager.Player;

		SetCurentAttack(player.BaseAttacks[0]);
		SelectEnemy();
    }

	public void GuardButton()
    {
		CombatManager combatManager = CombatManager.Instance;
		Player player = combatManager.Player;

		combatManager.EntityGuard(player);
	}

	public void AdrenalineButton()
	{
		DisplayPlayerOptions(false);
		StartCoroutine(WaitAndSpinWheel());
	}

	private IEnumerator WaitAndSpinWheel()
	{
		m_spinningwheel.gameObject.SetActive(true);

		yield return new WaitForSeconds(0.3f);

		m_spinningwheel.Spin();
	}

	private void RechargeAdrenaline()
	{
		if (m_isPickingSpell)
		{
			m_spinningwheel.gameObject.SetActive(false);
			m_spellScreen.SetActive(false);

			// Select new spell
			CombatManager.Instance.Player.AssignRandomSpell(out SO_Attack spell);
			AddStringToTextQueue($"You missed the bullet... your adrenaline has been restored.");
			AddStringToTextQueue($"You learned {spell.AttackName}!");

			if (m_shouldHealAfterSpin)
			{
				m_shouldHealAfterSpin = false;
				DoRandHeal();
			}
			else
			{
				m_isPickingSpell = false;
				OnSpellScreenEnd?.Invoke();
			}
		}
		else
		{
			// Hide UI
			DisplayPlayerOptions(false);
			m_spinningwheel.gameObject.SetActive(false);

			CombatManager.Instance.RechargeAdrenaline();
		}
	}

	#endregion

	#region Combat

	public void SetCurentAttack(SO_Attack newAttack)
	{
		m_currentAttack = newAttack;
	}

	private float m_selectionTime; // TO-DO: very bad solution, need a better way to offset whether selection is performed while within enemy selection
    public void SelectEnemy()
    {
		m_selectionTime = Time.realtimeSinceStartup;

        DisplayPlayerOptions(false);
        m_enemySelector.gameObject.SetActive(true);
        m_highlightedEnemy = 0;
        m_enemySelector.SelectEnemy(CombatManager.Instance.m_currentEncounter.Enemies[m_highlightedEnemy].gameObject.transform);

		m_inputActions.UI.Enable();
    }

    private void SelectEnemy(InputAction.CallbackContext ctx)
    {
		float input = ctx.ReadValue<Vector2>().x;

        if (input > 0f)
        {
            m_highlightedEnemy++;
            if (m_highlightedEnemy > CombatManager.Instance.m_currentEncounter.Enemies.Length - 1)
                m_highlightedEnemy = 0;
			
            while (m_currentEnemies[m_highlightedEnemy].IsDead)
            {
                m_highlightedEnemy++; 
				if (m_highlightedEnemy > CombatManager.Instance.m_currentEncounter.Enemies.Length - 1)
                    m_highlightedEnemy = 0;
            }

            m_enemySelector.SelectEnemy(CombatManager.Instance.m_currentEncounter.Enemies[m_highlightedEnemy].gameObject.transform);

        }

        if (input < 0f)
        {
            m_highlightedEnemy--;
            if (m_highlightedEnemy < 0)
                m_highlightedEnemy = CombatManager.Instance.m_currentEncounter.Enemies.Length - 1;

            while (m_currentEnemies[m_highlightedEnemy].IsDead)
            {
                m_highlightedEnemy--;
                if (m_highlightedEnemy < 0)
					m_highlightedEnemy = CombatManager.Instance.m_currentEncounter.Enemies.Length - 1;
            }
            
            m_enemySelector.SelectEnemy(CombatManager.Instance.m_currentEncounter.Enemies[m_highlightedEnemy].gameObject.transform);

        }
    }

    private void AttackEnemy(InputAction.CallbackContext ctx)
    {
		bool performAction = ctx.time > m_selectionTime + 0.1f; // TO-DO: same as sln above, needs fix

		if (performAction)
		{
			m_enemySelector.gameObject.SetActive(false);
			m_inputActions.UI.Disable();

			UseAttack(m_currentAttack);
			m_currentAttack = null;
		}
    }

    private void CancelEnemySelection(InputAction.CallbackContext ctx)
    {
        m_enemySelector.gameObject.SetActive(false);
        m_inputActions.UI.Disable();

        DisplayPlayerOptions(true);
    }
	#endregion

	#region Spell Selection

	private void ShowSpellSelectScreen()
	{
		m_spellScreen.SetActive(true);
		m_isPickingSpell = true;
	}

	public void YuhButton()
	{
		m_spellScreen.SetActive(false);
		m_shouldHealAfterSpin = true;
		StartCoroutine(WaitAndSpinWheel());
	}

	public void NahButton()
	{
		m_spellScreen.SetActive(false);
		DoRandHeal();
	}

	private void DoRandHeal()
	{
		float randHealChance = UnityEngine.Random.Range(0f, 1f);
		if (randHealChance > 0.65f)
		{
			int healAmount = UnityEngine.Random.Range(1, 5);
			CombatManager.Instance.Player.AddHealth(healAmount);

			string[] jokes =
			{
				"Yo, that wizard dropped some arcane amphetamines!",
				"The foe dropped some elvin moon dust... a puff of crystal dust fills air.",
				"A can of malicious brew rolls your way... you take a sip.",
				"You find a rolled parchment stuffed with glowing herbs. You light it and put it to your mouth."
			};
			string randomLine = jokes[UnityEngine.Random.Range(0, jokes.Length)];

			AddStringToTextQueue(randomLine);
			AddStringToTextQueue($"You regained {healAmount} health!");
			PlayTextQueue();
		}
		else
		{
			m_isPickingSpell = false;
			OnSpellScreenEnd?.Invoke();
		}
	}

	#endregion

	#region Final Game Screens

	private void FireBullet()
	{
		m_whiteLightPanel.SetActive(true);

		// Play tinitus ringing sound
		m_sfxSource.PlayOneShot(m_ringing);
		m_musicSource.Stop();
	}

	public void GameOver()
	{
		m_gameOverPanel.SetActive(true);

		m_sfxSource.PlayOneShot(m_death);
		m_musicSource.Stop();
	}

	public void Won()
	{
		m_playerWonPanel.SetActive(true);

		m_sfxSource.PlayOneShot(m_won);
		m_musicSource.Stop();
	}

	#endregion

	private void UseAttack(SO_Attack attackData)
    {
		CombatManager combatManager = CombatManager.Instance;

		// Perform attack on enemy
		combatManager.AttackEntity(combatManager.Player, m_currentEnemies[m_highlightedEnemy], attackData);

		m_particleEffects.SelectEnemy(m_currentEnemies[m_highlightedEnemy].gameObject.transform);
		m_particleSystem.Play();

		m_sfxSource.clip = attackData.SoundEffect;
        m_sfxSource.Play();
        DisplayPlayerOptions(false); // TO-DO: this can probably be removed
	}
}
