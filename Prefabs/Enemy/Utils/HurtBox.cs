using UnityEngine;

public class HurtBox : MonoBehaviour
{
    [SerializeField] private ConstructorConjunction constructors;
    [SerializeField] private EnemyHealth enemyHealth;

    private void Awake()
    {
        enemyHealth = GetComponentInParent<EnemyHealth>();
        if(enemyHealth == null) { GetComponent<EnemyHealth>(); }
        constructors = GetComponentInParent<ConstructorConjunction>();
        if(constructors == null) { GetComponent<ConstructorConjunction>(); }
    }

    public int GetDamage()
    {
        return constructors.GetDamage();
    }

    //Use when spawning projectiles
    public void SetConstructors(ConstructorConjunction boaConstructor)
    {
        constructors = boaConstructor;
    }
    public void SetHealthScript(EnemyHealth healthScript)
    {
        enemyHealth = healthScript;
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

        playerHealth.LoseHP(this.GetDamage());
    }
}
