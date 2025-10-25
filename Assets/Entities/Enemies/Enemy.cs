using UnityEngine;

public class Enemy : Entity
{
	[SerializeField] private SO_EnemyStats m_baseStats;

	public override void Die()
	{
		Debug.Log("Player is dead");
	}
}
