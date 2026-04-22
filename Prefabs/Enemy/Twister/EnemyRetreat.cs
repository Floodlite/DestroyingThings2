using UnityEngine;
using UnityEngine.AI;

public class EnemyRetreat : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private GameObject player;
    [SerializeField] private Vector3 playerLocation;
    [SerializeField] private EnemyConstructor enemy;
    [SerializeField] private Player[] players;
    [SerializeField] private float retreatDistance = 6f;
    [SerializeField] private float navSampleRadius = 2f;
    [SerializeField] private bool retreatMode= false;
    [SerializeField] private ConstructorConjunction constructors;
    


    private void Start()
    {
        agent.speed = enemy.enemySpeed;
    }

    private void Awake()
    {
        constructors = GetComponent<ConstructorConjunction>();
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
        
        if(Vector3.Distance(transform.position, closestPlayer.transform.position) < constructors.GetMinDistance()) { retreatMode = true; }
        else { retreatMode = false; }

        if(retreatMode) {
            Vector3 fromPlayer = (transform.position - closestPlayer.transform.position).normalized;
            Vector3 retreatTarget = transform.position + fromPlayer * retreatDistance;

            if(NavMesh.SamplePosition(retreatTarget, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
        else
        {
            if (NavMesh.SamplePosition(playerLocation, out NavMeshHit hit, navSampleRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }

        //TODO: Make Twister move back in range of player to attack after retreating
    }
}

