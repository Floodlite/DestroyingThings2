using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 8;
    [SerializeField] private int health = 8;
    [SerializeField] private PlayerScoreHandler scoreHandler;

    private void Awake()
    {
        scoreHandler = GetComponent<PlayerScoreHandler>();
    }

    private void Start()
    {
        ResetHP();
    }

    private void ResetHP()
    {
        health = maxHealth;
    }

    public void LoseHP(int healthLoss)
    {
        health -= healthLoss;
        Debug.Log("Player " + health);
        if (health <= 0)
        {
            Death();
        }
    }

    public void GainHP(int healthGain)
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

    private void Death()
    {
        Debug.Log("Failure failure failure");
        if(scoreHandler != null)
        {
            scoreHandler.MultiplyPoints(0.8f);
        }
        Destroy(gameObject);
    }
}
