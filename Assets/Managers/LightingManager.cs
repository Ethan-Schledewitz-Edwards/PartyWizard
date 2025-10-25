using UnityEngine;

public class FogManager : MonoBehaviour
{
	public float RGBScroll = 1;

	void Start()
	{
		
	}

	void Update()
	{
		float time = Time.time * RGBScroll;

		Color rainbow = Color.HSVToRGB(time % 1f, 1f, 1f);
		RenderSettings.fogColor = rainbow;
	}
}
