using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public void OnDieEnd()
	{
		UIManager.Instance.GameOver();
	}
}
