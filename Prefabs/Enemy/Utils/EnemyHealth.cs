using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private EnemyConstructor enemy;
    [SerializeField] private int maxHealth = 8;
    [SerializeField] private int health = 8;
    [SerializeField] private GameObject player;
    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private Player playerScript;


    private void Start()
    {
        maxHealth = enemy.enemyHealth;
        ResetHP();
        playerAttack = player.GetComponent<PlayerAttack>();
        playerScript = player.GetComponent<Player>();
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
        if (health <= 0)
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
    
    public void Ressurect() {
        gameObject.SetActive(true);
        transform.parent.gameObject.SetActive(true);
        maxHealth /= 2;
        ResetHP();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Punch"))
        {
            LoseHP(playerScript.attackDamage);
        }
    }
}
