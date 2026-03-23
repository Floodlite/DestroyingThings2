using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 8f;
    [SerializeField] private float health = 8f;

    private void Start()
    {
        resetHP();
    }


    private void resetHP()
    {
        health = maxHealth;
    }

    public void loseHP(int healthLoss)
    {
        health -= healthLoss;
        Debug.Log("Player " + health);
        if (health <= 0)
        {
            death();
        }
    }

    public void gainHP(int healthGain)
    {
        if (health + healthGain > maxHealth)
        {
            health = maxHealth;
        }
        else
        {
            health += healthGain;
        }
        Debug.Log("Player " + health);
    }

    private void death()
    {
        Debug.Log("Failure failure failure");
        Destroy(gameObject);
    }
}
