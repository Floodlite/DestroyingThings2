using UnityEngine;

public class HurtBox : MonoBehaviour
{
    public EnemyConstructor enemy;

    private void Start()
    {
        
    }

    public int GetDamage()
    {
        return enemy.enemyDamage;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null)
            return;

        playerHealth.LoseHP(this.GetDamage());
    }
}
