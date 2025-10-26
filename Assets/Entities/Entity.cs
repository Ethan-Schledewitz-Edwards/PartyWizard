using System;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
	[Header("Stats")]
	public int Health { get; private set; }
	public int MaxHealth { get; private set; } = 10;

	[Header("Attacks")]
	[field: SerializeField] public SO_Attack[] BaseAttacks { get; protected set; }

	// System
	public bool IsDead;

	protected virtual void Start()
	{
		Health = MaxHealth;
	}

	public void SetHealth(int value)
	{
		Health = Mathf.Clamp(value, 0, MaxHealth);

		if(Health <= 0)
		{
			IsDead = true;
			Die();
		}
	}

	public abstract void Die();

	public void RemoveHealth(int value, out bool isDead)
	{
		SetHealth(Health - value);
		isDead = IsDead;
	} 

	public void AddHealth(int value) => SetHealth(Health + value);
}
