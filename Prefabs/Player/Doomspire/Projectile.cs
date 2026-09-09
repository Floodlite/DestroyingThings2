using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private int damage;
    [SerializeField] private EnemyHealth enemyHealth;


    public void SetDamage(int toSet)
    {
        damage = toSet;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(!collision.collider.CompareTag("Enemy")) { return; }
        enemyHealth = collision.collider.GetComponentInChildren<EnemyHealth>();
        if(enemyHealth==null) { return; }
        enemyHealth.LoseHP(damage);
    }

    public void YouHaveBeenDestroyed()
    {
        Destroy(this.GetComponent<Projectile>());
    }
}
