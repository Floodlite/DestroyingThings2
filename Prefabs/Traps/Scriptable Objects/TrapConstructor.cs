using UnityEngine;

[CreateAssetMenu(fileName = "New Trap", menuName = "Create Trap")]
public class TrapConstructor : ScriptableObject
{
    public new string name;
    public int trapDamage;
    public float armTime;

}
