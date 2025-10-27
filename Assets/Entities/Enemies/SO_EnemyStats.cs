using UnityEngine;

[CreateAssetMenu(fileName = "SO_EnemyStats", menuName = "Scriptable Objects/SO_EnemyStats")]
public class SO_EnemyStats : ScriptableObject
{
	[field: SerializeField] public string EnemyName { get; private set; } = "EnemyName";
	[field: SerializeField] public int BaseHealth { get; private set; }
	[field: SerializeField] public SO_Attack[] Attacks { get; private set; }
	[field: SerializeField] public bool IsBoss { get; private set; }
}
