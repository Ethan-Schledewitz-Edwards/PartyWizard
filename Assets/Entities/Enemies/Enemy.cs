using UnityEngine;

public class Enemy : Entity
{
	[SerializeField] private SO_EnemyStats m_baseStats;

	private void Awake()
	{
		BaseAttacks = m_baseStats.Attacks;
	}

	public override void Die()
	{
		Debug.Log("Player is dead");
	}
}
