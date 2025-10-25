using UnityEngine;

public abstract class Entity : MonoBehaviour
{
	public int Health { get; private set; }
	public int MaxHealth { get; private set; } = 10;

	protected virtual void Start()
	{
		Health = MaxHealth;
	}

	public void SetHealth(int value)
	{
		Health = Mathf.Clamp(value, 0, MaxHealth);
	}

	public void RemoveHealth(int value) => SetHealth(Health - value);

	public void AddHealth(int value) => SetHealth(Health + value);
}
