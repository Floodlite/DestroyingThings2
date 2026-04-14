using UnityEngine;

[CreateAssetMenu(fileName = "New Ranged Enemy Settings", menuName = "Create Ranged Enemy Settings")]
public class RangedEnemyConstructor : ScriptableObject
{
    public new string name;
    public float minPlayerDistance;
    public float maxPlayerDistance;
    public float attackFrequency;
    public float projectileSpeed;
    public float projectileLifespan;
    public GameObject projectile;
}
