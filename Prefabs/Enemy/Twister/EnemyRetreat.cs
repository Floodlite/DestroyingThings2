using UnityEngine;
using UnityEngine.AI;

public class EnemyRetreat : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private GameObject player;
    [SerializeField] private Vector3 playerLocation;
    [SerializeField] private EnemyConstructor enemy;

    void Start()
    {
        agent.speed = enemy.enemySpeed;
    }

    void Update()
    {
        playerLocation = player.transform.position * -1f;
        agent.SetDestination(playerLocation);
    }
}
