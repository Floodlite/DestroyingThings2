using UnityEngine;
using UnityEngine.AI;

public class EnemyChase : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private GameObject player;
    [SerializeField] private Vector3 playerLocation;
    [SerializeField] private EnemyConstructor enemy;
    [SerializeField] private Player[] players;
    [SerializeField] private float navSampleRadius = 2f;


    private void Start()
    {
        agent.speed = enemy.enemySpeed;
    }

    private Player FindClosestPlayer()
    {
        float distanceToPlayer;
        float closestDistance = 99999;
        int indexOfClosest = 0;

        //Switch to FindObjectsSortMode.None if performance issues arise
        players = FindObjectsByType<Player>(FindObjectsSortMode.InstanceID);
        if (players == null || players.Length == 0) { return null; }

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

    private void Update()
    {
        Player closestPlayer = FindClosestPlayer();
        if(closestPlayer == null) {
            agent.ResetPath(); 
            return; 
        }

        playerLocation = closestPlayer.transform.position;

        if (NavMesh.SamplePosition(playerLocation, out NavMeshHit hit, navSampleRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
}

