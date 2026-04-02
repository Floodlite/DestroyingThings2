using UnityEngine;

public class HurtBox : MonoBehaviour
{
    public EnemyConstructor enemy;
    [SerializeField] private EnemyHealth enemyHealth;

    private void Awake()
    {
        if (enemyHealth == null)
        {
            enemyHealth = GetComponentInParent<EnemyHealth>();
        }
    }

    public int GetDamage()
    {
        return enemy.enemyDamage;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (enemyHealth != null && enemyHealth.IsDead)
        {
            return;
        }

        if (!other.CompareTag("Player")) {
            return;
        }

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null) {
            return;
        }

        playerHealth.LoseHP(GetDamage());
    }
}
