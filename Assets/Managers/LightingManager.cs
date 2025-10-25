using UnityEngine;

public class FogManager : MonoBehaviour
{
	[SerializeField] private float m_RGBScroll = 1;

	void Start()
	{
		
	}

	void Update()
	{
		float time = Time.time * m_RGBScroll;

		Color rainbow = Color.HSVToRGB(time % 1f, 1f, 1f);
		RenderSettings.fogColor = rainbow;
	}
}
