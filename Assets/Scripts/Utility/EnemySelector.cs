using UnityEngine;

public class EnemySelector : MonoBehaviour
{
    public void SelectEnemy(Transform enemy)
    {
        Vector3 target = enemy.position;

        target.y = 1f; // position to be placed just above the enemy

        transform.position = target;
    }
}
