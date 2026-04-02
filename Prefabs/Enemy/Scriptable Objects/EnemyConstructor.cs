using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Create Enemy")]
public class EnemyConstructor : ScriptableObject
{
    public new string name;
    public int enemyDamage;
    public int enemySpeed;
    public int enemyHealth;
    public Sprite image;

}
