using UnityEngine;

public class Reanimator : MonoBehaviour
{
    [SerializeField] private bool dead = false;
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
