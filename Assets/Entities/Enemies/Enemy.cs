using UnityEngine;

public class Enemy : Entity
{
	[field: SerializeField] public SO_EnemyStats BaseStats { get; private set; }

	private void Awake()
	{
		MaxHealth = BaseStats.BaseHealth;
		SetHealth(BaseStats.BaseHealth);
		BaseAttacks = BaseStats.Attacks;
	}

	public override void Die()
	{
		gameObject.SetActive(false);
	}
}
