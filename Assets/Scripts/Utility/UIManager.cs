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
	[SerializeField] private SpellButton m_spellButton;
	[SerializeField] private Slider m_healthSlider;
	[SerializeField] private Slider m_adrenalineSlider;
	[SerializeField] private SpinningWheel m_spinningwheel;

	[SerializeField] private EnemySelector m_enemySelector;

	[Header("Text Box")]
	[SerializeField] private GameObject m_textPanel;
	[SerializeField] private TextMeshProUGUI m_textBox;

	[HideInInspector] public SO_Attack m_selectedAttack;
	private InputSystem_Actions m_inputActions;

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

			// pause between messages
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

		m_selectedAttack = player.BaseAttacks[0];
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

	private void FireBullet()
	{
		Debug.Log("YOU DIED");
	}

	private void RechargeAdrenaline()
	{
		// Hide UI
		DisplayPlayerOptions(false);
		m_spinningwheel.gameObject.SetActive(false);

		CombatManager.Instance.RechargeAdrenaline();
	}

	#endregion

	#region Enemy Selection

	private float m_selectionTime; // TO-DO: very bad solution, need a better way to offset whether selection is performed while within enemy selection
    public void SelectEnemy()
    {
		m_selectionTime = Time.realtimeSinceStartup;

        DisplayPlayerOptions(false);
        m_enemySelector.gameObject.SetActive(true);
        m_highlightedEnemy = 0;
        m_enemySelector.SelectEnemy(CombatManager.Instance.currentEncounter.Enemies[m_highlightedEnemy].gameObject.transform);

		m_inputActions.UI.Enable();
    }

    private void SelectEnemy(InputAction.CallbackContext ctx)
    {
		float input = ctx.ReadValue<Vector2>().x;

        if (input > 0f)
        {
            m_highlightedEnemy++;
            if (m_highlightedEnemy > CombatManager.Instance.currentEncounter.Enemies.Length - 1)
                m_highlightedEnemy = 0;
			
            while (m_currentEnemies[m_highlightedEnemy].IsDead)
            {
                m_highlightedEnemy++; 
				if (m_highlightedEnemy > CombatManager.Instance.currentEncounter.Enemies.Length - 1)
                    m_highlightedEnemy = 0;
            }

            m_enemySelector.SelectEnemy(CombatManager.Instance.currentEncounter.Enemies[m_highlightedEnemy].gameObject.transform);

        }

        if (input < 0f)
        {
            m_highlightedEnemy--;
            if (m_highlightedEnemy < 0)
                m_highlightedEnemy = CombatManager.Instance.currentEncounter.Enemies.Length - 1;

            while (m_currentEnemies[m_highlightedEnemy].IsDead)
            {
                m_highlightedEnemy--;
                if (m_highlightedEnemy < 0)
					m_highlightedEnemy = CombatManager.Instance.currentEncounter.Enemies.Length - 1;
            }
            
            m_enemySelector.SelectEnemy(CombatManager.Instance.currentEncounter.Enemies[m_highlightedEnemy].gameObject.transform);

        }
    }

    private void AttackEnemy(InputAction.CallbackContext ctx)
    {
		bool performAction = ctx.time > m_selectionTime + 0.1f; // TO-DO: same as sln above, needs fix

		if (performAction)
		{
			m_enemySelector.gameObject.SetActive(false);
			m_inputActions.UI.Disable();

			UseAttack(m_selectedAttack);
		}
    }

    private void CancelEnemySelection(InputAction.CallbackContext ctx)
    {
        m_enemySelector.gameObject.SetActive(false);
        m_inputActions.UI.Disable();

        DisplayPlayerOptions(true);
    }
    #endregion
    public void UseAttack(SO_Attack attackData)
    {
		CombatManager combatManager = CombatManager.Instance;

		// Perform attack on enemy
		combatManager.AttackEntity(combatManager.Player, m_currentEnemies[m_highlightedEnemy], attackData);

		DisplayPlayerOptions(false); // TO-DO: this can probably be removed
	}
}
