using UnityEngine;

public class EnemySelector : MonoBehaviour
{
    [SerializeField] private float targetY;

    public void SelectEnemy(Transform enemy)
    {
        Vector3 target = enemy.position;

        target.y = targetY; // position to be placed along the y-axis

        transform.position = target;
    }
}
