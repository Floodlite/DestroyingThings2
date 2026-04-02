using UnityEngine;

public class TrapHurtBox : MonoBehaviour
{
    public TrapConstructor trap;


    public int GetTrapDamage()
    {
        return trap.trapDamage;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") && !other.CompareTag("Enemy")) {
            return;
        }
        Debug.Log("Exploded: " + other.name);
        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
        EnemyHealth enemyHealth = other.GetComponentInChildren<EnemyHealth>();
        if (playerHealth != null) {
            playerHealth.LoseHP(this.GetTrapDamage());
        }
        if (enemyHealth != null) {
            enemyHealth.LoseHP(this.GetTrapDamage());
        }
        

        
    }
}
