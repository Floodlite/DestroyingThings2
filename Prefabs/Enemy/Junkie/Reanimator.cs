using UnityEngine;

public class Reanimator : MonoBehaviour
{
    [SerializeField] private bool dead = false;
    [SerializeField] private bool resurrected = false; //Whether or not this enemy has previously been brought back from the dead
    private EnemyHealth controlledEnemy;

    private void Awake()
    {
        if (controlledEnemy == null)
        {
            controlledEnemy = GetComponentInChildren<EnemyHealth>(true);
        }
    }

    public void SetDeathStatus(bool aliveOrDead)
    {
        dead = aliveOrDead;
        //Debug.Log("Switched to " + aliveOrDead);
    }

    public bool IsDead()
    {
        return dead;
    }

    public void SetResStatus(bool newStatus)
    {
        resurrected = newStatus;
    }

    public bool GetResStatus()
    {
        return resurrected;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!dead || !other.CompareTag("Junkie"))
        {
            return;
        }

        if (controlledEnemy == null)
        {
            controlledEnemy = GetComponentInChildren<EnemyHealth>(true);
        }

        if (controlledEnemy == null)
        {
            return;
        }

        controlledEnemy.Resurrect();
    }
}
