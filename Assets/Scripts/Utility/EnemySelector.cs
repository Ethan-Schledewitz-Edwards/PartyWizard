using UnityEngine;

public class EnemySelector : MonoBehaviour
{
    public void SelectEnemy(Transform enemy)
    {
        Vector3 target = enemy.position;

        target.y = -1.72f; // position to be placed just above ground

        transform.position = target;
    }
}
