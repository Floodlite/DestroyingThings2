using UnityEngine;

[CreateAssetMenu(fileName = "New Effect", menuName = "Create Effect")]
public class BoosterBuilder : ScriptableObject
//Unrelated idea: Change name of EnemyConstructor to FoeFactory
{
    public string effectName;
    public float effectDuration; //in seconds
    public int damageBoost;
    public float damageMultiplier;
    public float speedBoost;
    public float speedMultiplier;
    public float attackCooldownMultiplier;


}
