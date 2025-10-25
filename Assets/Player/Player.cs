using UnityEngine;

public class Player : Entity
{
	public int Adrenaline { get; private set; }
	public int MaxAdrenaline { get; private set; } = 10;

	public void SetAdrenaline(int value)
	{
		Adrenaline = Mathf.Clamp(value, 0, MaxAdrenaline);
	}

	public void RemoveAdrenaline(int value) => SetAdrenaline(Adrenaline - value);

}
