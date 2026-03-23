using UnityEngine;
using UnityEngine.AI;

public class RoadblockChase : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private float playerLocationX;
    [SerializeField] private EnemyConstructor enemy;
    private Vector3 startLocation;
    [SerializeField] private Player[] players;

    void Start()
    {
        agent.speed = enemy.enemySpeed;
        startLocation = transform.position;
    }

    private Player FindClosestPlayer()
    {
        float distanceToPlayer = 0;
        float closestDistance = 99999;
        int indexOfClosest = 0;

        //Switch to FindObjectsSortMode.None if performance issues arise
        players = FindObjectsByType<Player>(FindObjectsSortMode.InstanceID);

        for(int i=0; i<players.Length; i++)
        {
            distanceToPlayer = Vector3.Distance(transform.position, players[i].transform.position);
            
            if(distanceToPlayer < closestDistance)
            {
                closestDistance = distanceToPlayer;
                indexOfClosest = i;
            }
        }
        return players[indexOfClosest];
    }

    void Update()
    {
        playerLocationX = FindClosestPlayer().transform.position.x;
        agent.SetDestination(new Vector3 (playerLocationX, startLocation.y, startLocation.z));
    }
}
