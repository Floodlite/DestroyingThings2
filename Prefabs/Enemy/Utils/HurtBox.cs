using UnityEngine;

public class HurtBox : MonoBehaviour
{
    public EnemyConstructor enemy;
    public GameObject player;
    public PlayerHealth playerHealth;

    void Start()
    {
        playerHealth = player.GetComponent<PlayerHealth>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerHealth.loseHP(enemy.enemyDamage);
            //Cool idea: If you are moving faster than the enemy, you will only take half the damage
        }
    }
}
