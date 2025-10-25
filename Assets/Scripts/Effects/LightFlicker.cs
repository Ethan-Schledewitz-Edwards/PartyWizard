using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour
{
	private Light m_lightComponent;
	private float m_minTime = .2f;
	private float m_maxTime = 2f;
	private float m_flickerTimer;

	private void Start()
	{
		m_lightComponent = GetComponent<Light>();

		StartCoroutine(Flicker());
	}



	IEnumerator Flicker()
	{
		m_lightComponent.enabled = true;
		float m_flickerTimer = Random.Range(m_minTime, m_maxTime);
		yield return new WaitForSeconds(m_flickerTimer);
		m_lightComponent.enabled = false;
		yield return new WaitForSeconds(.2f);
		StartCoroutine(Flicker());
	}
}
