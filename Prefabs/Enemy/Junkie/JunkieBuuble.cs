using UnityEngine;

public class JunkieBubble : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy"))
        {
            return;
        }

        EnemyHealth enemyHealth = other.GetComponentInParent<EnemyHealth>();
        if (enemyHealth == null)
        {
            enemyHealth = other.GetComponentInChildren<EnemyHealth>(true);
        }

        if (enemyHealth == null)
        {
            return;
        }

        enemyHealth.Resurrect();
    }
}
