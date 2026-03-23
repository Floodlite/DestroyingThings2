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
        /*
        if (other.CompareTag("Player"))
        {
            playerHealth.LoseHP(enemy.enemyDamage);
            //Cool idea: If you are moving faster than the enemy, you will only take half the damage
        }
        */
        Player player;
        PlayerHealth playerHealth;

        player = other.GetComponentInParent<Player>();
        playerHealth = player.GetComponentInParent<PlayerHealth>();

        if(player != null)
        {
            playerHealth.LoseHP(this.GetDamage());
        }
    }
}
