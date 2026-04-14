using UnityEngine;

public class ConstructorConjunction : MonoBehaviour
{
    [SerializeField] private EnemyConstructor enemy;
    [SerializeField] private RangedEnemyConstructor rangedEnemy;

    public EnemyConstructor GetEnemy()
    {
        return enemy;
    }

    public RangedEnemyConstructor GetRangedEnemy()
    {
        if(rangedEnemy == null) { return null; }
        return rangedEnemy;
    }

    public int GetDamage()
    {
        return enemy.enemyDamage;
    }

    public int GetMaxHealth()
    {
        return enemy.enemyHealth;
    }

    public int GetSpeed()
    {
        return enemy.enemySpeed;
    }

    public string GetName()
    {
        return enemy.name;
    }

    public Sprite GetSprite()
    {
        return enemy.image;
    }

    /// <summary>
    /// Minimum distance a player needs to be away from this enemy for it to attack
    /// <para>(Think of it like the enemy's "personal space")</para>
    /// </summary>
    public float GetMinDistance()
    {
        if(rangedEnemy == null) { return 5f; }
        return rangedEnemy.minPlayerDistance;
    }

    /// <summary>
    /// Minimum distance a player can be away from this enemy for it to attack
    /// </summary>
    public float GetMaxDistance()
    {
        if(rangedEnemy == null) { return 70f; }
        return rangedEnemy.maxPlayerDistance;
    }

    public float GetAttackFreq()
    {
        if(rangedEnemy == null) { return 5f; }
        return rangedEnemy.attackFrequency;
    }

    public float GetProjectileSpeed()
    {
        if(rangedEnemy == null) { return 5f; }
        return rangedEnemy.projectileSpeed;
    }

    public float GetProjectileLifespan()
    {
        if(rangedEnemy == null) { return 5f; }
        return rangedEnemy.projectileLifespan;
    }

    public GameObject GetProjectile()
    {
        if(rangedEnemy == null) { return null; }
        return rangedEnemy.projectile;
    }

    public Rigidbody GetProjectileRb()
    {
        if(rangedEnemy == null) { return null; }
        return rangedEnemy.projectile.GetComponent<Rigidbody>();
    }
    

    // public new string name;
    // public int enemyDamage;
    // public int enemySpeed;
    // public int enemyHealth;
    // public Sprite image;
 
    // public float minPlayerDistance;
    // public float maxPlayerDistance;
    // public float attackFrequency;
    // public float projectileSpeed;
    // public GameObject projectile;
}
