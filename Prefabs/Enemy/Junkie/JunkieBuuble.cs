using UnityEngine;

public class JunkieBubble : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        EnemyHealth enemyHealth;
        enemyHealth = other.GetComponentInChildren<EnemyHealth>();

        if (!other.CompareTag("Enemy")) { 
            return;         
        }
    
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
