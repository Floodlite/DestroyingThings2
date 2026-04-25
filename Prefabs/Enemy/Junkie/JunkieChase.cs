using UnityEngine;
using UnityEngine.AI;

public class JunkieChase : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private GameObject player;
    [SerializeField] private Vector3 targetLocation;
    [SerializeField] private EnemyConstructor enemy;
    [SerializeField] private Player[] players;
    [SerializeField] private Reanimator[] corpses;
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

    /// <summary>
    /// Returns the transform location of the nearest dead enemy
    /// <para>Returns null if everyone is alive</para>
    /// </summary>
    private Transform FindClosestCorpse()
    {
        float distanceToCorpse;
        float closestDistance = 99999;
        int indexOfClosest = 0;

        //Note: Switch to FindObjectsSortMode.None if performance issues arise
        corpses = FindObjectsByType<Reanimator>(FindObjectsSortMode.InstanceID);
        if (corpses == null || corpses.Length == 0) { return null; }

        for(int i=0; i<corpses.Length; i++)
        {
            Reanimator reanimator = corpses[i].GetComponentInParent<Reanimator>();
            if (reanimator == null) {
                continue;
            }
            if(!reanimator.IsDead())
            {
                continue;
            }

            distanceToCorpse = Vector3.Distance(transform.position, corpses[i].transform.position);
            
            if(distanceToCorpse < closestDistance)
            {
                closestDistance = distanceToCorpse;
                indexOfClosest = i;
            }
        }
        return corpses[indexOfClosest].transform;
    } 

    private void Update()
    {
        if(FindClosestCorpse() == null) {
            Player closestPlayer = FindClosestPlayer();
            if(closestPlayer == null) {
                agent.ResetPath(); 
                return; 
            }
            targetLocation = closestPlayer.transform.position;
        }
        else
        {
            targetLocation = FindClosestCorpse().position;
            if(targetLocation == null) {
                agent.ResetPath(); 
                return; 
            }
        }

        if (NavMesh.SamplePosition(targetLocation, out NavMeshHit hit, navSampleRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
}

