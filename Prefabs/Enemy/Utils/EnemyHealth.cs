using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private EnemyConstructor enemy;
    [SerializeField] private int maxHealth = 8;
    [SerializeField] private int health = 8;


    private void Start()
    {
        maxHealth = enemy.enemyHealth;
        ResetHP();
        //playerAttack = player.GetComponent<PlayerAttack>();
        //playerScript = player.GetComponent<Player>();
    }

    public int RetrieveHP(bool returnHP)
    {
        if(returnHP)
        {
            return health;
        }
        else
        {
            return maxHealth;
        }
        
    }

    private void ResetHP()
    {
        health = maxHealth;
    }

    public void LoseHP(int healthLoss)
    {
        health -= healthLoss;
        Debug.Log("Enemy " + this.name  + " " + health);
        if (health <= 0.0)
        {
            Death();
        }
    }

    private void Death()
    {
        //Destroy(gameObject);
        //Destroy(transform.parent.gameObject);
        gameObject.SetActive(false);
        transform.parent.gameObject.SetActive(true);
    }
    
    public void Resurrect () {
        gameObject.SetActive(true);
        transform.parent.gameObject.SetActive(true);
        maxHealth *= 2/3;
        ResetHP();
    }

    //Makes this enemy lose health when getting punched
    private void OnTriggerEnter(Collider other)
    {
        Player player;
        player = other.GetComponentInParent<Player>();

        if (!other.CompareTag("Punch")) { return; }

        if(player != null)
        {
            LoseHP(player.GetDamage());
        }
    }
}
