using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
	public static UIManager Instance;

	[Header("Player Options")]
	[SerializeField] private GameObject m_canvas;
	[SerializeField] private GameObject m_playerOptions;
    [SerializeField] private GameObject m_basePanel;
    [SerializeField] private GameObject m_itemPanel;
	[SerializeField] private SpellButton m_spellButton;
	[SerializeField] private Slider m_adrenalineSlider;

	[Header("Text Box")]
	[SerializeField] private GameObject m_textPanel;
	[SerializeField] private TextMeshProUGUI m_textBox;

	// System
	private Encounter m_currentEncounter;
	private Enemy[] m_currentEnemies;

	public bool IsTextPrinting { get; private set; }
	public Action OnTextFinished;

	#region Initialization

	private void Awake()
	{
		if (Instance == null)
			Instance = this;
		else
			Destroy(this);
	}

	private void Start()
	{
		m_canvas.SetActive(false);
		m_playerOptions.SetActive(false);
		m_basePanel.SetActive(true);
		m_itemPanel.SetActive(false);
		m_textPanel.SetActive(false);

		CombatManager.Instance.OnEncounterBegin += BeginEncounter;
		CombatManager.Instance.OnEncounterEnd += EndEncounter;
		CombatManager.Instance.OnAttackPerformed += DisplayTextPanel;
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
        m_currentEncounter = encounter;
		m_currentEnemies = encounter.Enemies;

		m_canvas.SetActive(true);
		DisplayPlayerOptions(true);
	}

	private void EndEncounter(Encounter encounter)
	{
		m_canvas.SetActive(false);
		DisplayPlayerOptions(false);
		DoneText();
	}

	public void DisplayTextPanel(string text)
	{
		DisplayPlayerOptions(false);
		m_textPanel.SetActive(true);

		StopAllCoroutines();
		IsTextPrinting = true;
		StartCoroutine(TypeText(text));
	}

	private IEnumerator TypeText(string text)
	{
		m_textBox.text = "";
		foreach (char c in text)
		{
			m_textBox.text += c;
			yield return new WaitForSeconds(0.03f);// typing speed
		}

		DoneText();
	}

	public void DoneText()
	{
		IsTextPrinting = false;
		m_textPanel.SetActive(true);
		OnTextFinished?.Invoke();
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

		Player player = CombatManager.Instance.Player;
		m_spellButton.SetSpell(player.CurrentSpell);
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
        // TO-DO: Guard
    }

    public void ItemButton()
    {
		ResetPlayerOptions();
	}

    #endregion
    
    public void UseAttack(SO_Attack attackData)
    {
		CombatManager combatManager = CombatManager.Instance;
		combatManager.AttackEntity(combatManager.Player, m_currentEnemies[0], attackData);// To-Do: use the highlighted enemy

		DisplayPlayerOptions(false);
	}

    public void BackButton()
    {
		ResetPlayerOptions();
	}
}
