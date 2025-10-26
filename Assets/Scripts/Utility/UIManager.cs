using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	public static UIManager Instance;

	[Header("Player Options")]
	[SerializeField] private GameObject m_canvas;
	[SerializeField] private GameObject m_playerOptions;
    [SerializeField] private GameObject m_basePanel;
    [SerializeField] private GameObject m_itemPanel;
	[SerializeField] private SpellButton m_spellButton;
	[SerializeField] private Slider m_healthSlider;
	[SerializeField] private Slider m_adrenalineSlider;

	[Header("Text Box")]
	[SerializeField] private GameObject m_textPanel;
	[SerializeField] private TextMeshProUGUI m_textBox;

	// System
	private Queue<string> m_stringsToType;
	private Enemy[] m_currentEnemies;
	private int m_highlightedEnemy;

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
		m_basePanel.SetActive(true);
		m_itemPanel.SetActive(false);
		m_textPanel.SetActive(false);

		m_healthSlider.maxValue = combatManager.Player.MaxHealth;
		m_adrenalineSlider.maxValue = combatManager.Player.MaxAdrenaline;

		combatManager.OnEncounterBegin += BeginEncounter;
		combatManager.OnEncounterEnd += EndEncounter;
	}
	#endregion

	private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (!m_basePanel.activeInHierarchy)
            {
				m_basePanel.SetActive(true);
				m_itemPanel.SetActive(false);
            }
        }
    }

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
		m_canvas.SetActive(false);
		DisplayPlayerOptions(false);
		HideTextPanel();
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

			// optional pause between messages
			yield return new WaitForSeconds(0.5f);
		}

		HideTextPanel();
		IsPrintingTextQueue = false;
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
		m_basePanel.SetActive(true);
		m_itemPanel.SetActive(false);

		// Refresh current spell
		Player player = CombatManager.Instance.Player;
		m_spellButton.SetSpell(player.CurrentSpell);

		// Refresh health and adrenaline bars
		m_healthSlider.value = player.Health;
		m_adrenalineSlider.value = player.Adrenaline;
	}

	#region Player Options

	public void AttackButton()
    {
		CombatManager combatManager = CombatManager.Instance;
		Player player = combatManager.Player;

		// The player should only ever have punch by default
		UseAttack(player.BaseAttacks[0]);
    }

	public void GuardButton()
    {
		CombatManager combatManager = CombatManager.Instance;
		Player player = combatManager.Player;

		combatManager.EntityGuard(player);
	}

    public void AdrenalineButton()
    {
		// Pull up revolver
	}

    #endregion
    
    public void UseAttack(SO_Attack attackData)
    {
		CombatManager combatManager = CombatManager.Instance;

		// Remove this once proper enemy highlights are implemented!!!
		foreach (Enemy i in m_currentEnemies)
		{
			if (i.IsDead)
				m_highlightedEnemy++;
		}

		// Perform attack on enemy
		combatManager.AttackEntity(combatManager.Player, m_currentEnemies[m_highlightedEnemy], attackData);

		DisplayPlayerOptions(false);
	}

    public void BackButton()
    {
		ResetPlayerOptions();
	}
}
