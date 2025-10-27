using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SpellButton : MonoBehaviour
{
	[field: SerializeField] public TextMeshProUGUI Text { get; set; }
	private SO_Attack AttackData {  get; set; }

	private Button m_button;

	private void Awake()
	{
		m_button = GetComponent<Button>();
	}

	public void SetSpell(SO_Attack attackData)
	{
		AttackData = attackData;
		Text.text = AttackData.AttackName;

		Player player = CombatManager.Instance.Player;
		m_button.interactable = player.Adrenaline >= attackData.AdrenalineCost;
	}

	public void CastSpell()
	{
        UIManager.Instance.SetCurentAttack(AttackData);
        UIManager.Instance.SelectEnemy();
    }
}
