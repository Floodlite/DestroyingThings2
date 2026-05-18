using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Create Enemy")]
public class EnemyConstructor : ScriptableObject
{
    public Sprite image;
    public new string name;
    public int enemyDamage;
    public int enemySpeed;
    public int enemyHealth;
    public float dangerValue;
    public float pointsValue;
    public bool special;
}
