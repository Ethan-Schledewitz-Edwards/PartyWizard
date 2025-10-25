using UnityEngine;

[CreateAssetMenu(fileName = "SO_Attack", menuName = "Scriptable Objects/SO_Attack")]
public class SO_Attack : ScriptableObject
{
	[field: SerializeField] public string AttackName { get; private set; } = "Attack";

	[Header("Stats")]
	[field: SerializeField] public int Damage { get; private set; } = 10;
	[field: SerializeField] public int AdrenalineCost { get; private set; } = 10;

	[Header("VFX")]
	[field: SerializeField] public float AttackDuration { get; private set; } = 1;
	[field: SerializeField] public AudioClip SoundEffect { get; private set; }
	[field: SerializeField] public GameObject ParticleSystem { get; private set; }
}
