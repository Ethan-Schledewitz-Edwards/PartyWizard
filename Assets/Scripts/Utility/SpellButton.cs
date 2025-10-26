using TMPro;
using UnityEngine;

public class SpellButton : MonoBehaviour
{
	[field: SerializeField] public TextMeshProUGUI Text { get; set; }
	private SO_Attack AttackData {  get; set; }

	public void SetSpell(SO_Attack attackData)
	{
		AttackData = attackData;
		Text.text = AttackData.AttackName;
	}

	public void CastSpell()
	{
		UIManager.Instance.UseAttack(AttackData);
	}
}
