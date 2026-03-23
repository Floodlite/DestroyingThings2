using UnityEngine;
using UnityEngine.AI;

public class RoadblockChase : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private GameObject player;
    [SerializeField] private float playerLocationX;
    [SerializeField] private EnemyConstructor enemy;
    private Vector3 startLocation;

    void Start()
    {
        agent.speed = enemy.enemySpeed;
        startLocation = transform.position;
    }

    void Update()
    {
        playerLocationX = player.transform.position.x;
        agent.SetDestination(new Vector3 (playerLocationX, startLocation.y, startLocation.z));
    }
}
