using UnityEngine;
using UnityEngine.AI;

public class EnemyRetreat : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private GameObject player;
    [SerializeField] private Vector3 playerLocation;
    [SerializeField] private EnemyConstructor enemy;
    [SerializeField] private Player[] players;
    [SerializeField] private float retreatDistance = 16f;
    [SerializeField] private float navSampleRadius = 4f;
    [SerializeField] private bool retreatMode = true;
    [SerializeField] private ConstructorConjunction constructors;
    private float agentSpeed;
    

    private void Awake()
    {
     
        constructors = GetComponent<ConstructorConjunction>();
    }

    private void Start()
    {
        agentSpeed = constructors.GetSpeed();
        agent.speed = agentSpeed;
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
        
        float distance = Vector3.Distance(this.transform.position, closestPlayer.transform.position);
        //Debug.Log(distance);
        if(distance * constructors.GetRetreatMultiplier() < constructors.GetMinDistance()) { 
            retreatMode = true; 
        }
        else { 
            retreatMode = false; 
        }

        if(retreatMode) {
            Vector3 fromPlayer = (transform.position - closestPlayer.transform.position).normalized;
            Vector3 retreatTarget = transform.position + fromPlayer * retreatDistance;

            if(NavMesh.SamplePosition(retreatTarget, out NavMeshHit hit, navSampleRadius, NavMesh.AllAreas))
            {
                agent.speed *= 2.5f;
                agent.SetDestination(hit.position);
            }
        }
        else
        {
            agent.speed = agentSpeed;
            if (NavMesh.SamplePosition(playerLocation, out NavMeshHit hit, navSampleRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }

        //TODO: Make Twister move back in range of player to attack after retreating
    }
}

