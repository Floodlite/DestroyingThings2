using UnityEngine;
using UnityEngine.AI;

public class EnemyChase : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private GameObject player;
    [SerializeField] private Vector3 playerLocation;
    [SerializeField] private Vector3 targetLocation;
    [SerializeField] private Player[] players;
    [SerializeField] private Reanimator[] corpses;
    [SerializeField] private float navSampleRadius = 4f;
    [SerializeField] private float retreatDistance = 16f;
    [SerializeField] private bool retreatMode = true;
    [SerializeField] private ConstructorConjunction constructors;
    private float agentSpeed;

    public enum EnemyRole
    {
        BASE=0, //"base" in all lowercase was already taken, but I don't feel like lengthening it to baseEnemy
        CHASE=1,
    };
    [SerializeField] private EnemyRole enemyRole = EnemyRole.CHASE;
    //TODO: Merge EnemyRetreat and JunkieChase logic into this script
    private enum ChaseMode
    {
        pursue=0,
        retreat=1,
        res=2,
    }
    [SerializeField] private ChaseMode chaseMode = ChaseMode.pursue;
    

    private void Awake()
    {
        constructors = GetComponent<ConstructorConjunction>();
    }

    private void Start()
    {
        agent.enabled = false;
        agent.Warp(transform.position);
        agent.enabled = true;

        if(constructors != null) {
            agent.speed = constructors.GetSpeed();
        }
        else
        {
            agent.speed = 15f;
        }
        agentSpeed = agent.speed;
    }

    public void SwitchRole(EnemyRole role)
    {
        enemyRole = role;
        switch(role)
        {
            case EnemyRole.CHASE:
                UpdateAgent(2);
                break;
            case EnemyRole.BASE:
                UpdateAgent(0);
                break;
            default:
                UpdateAgent(0);
                break;
        } 
    }

    public EnemyRole GetEnemyRole()
    {
        return enemyRole;
    }

    private void UpdateAgent(int index)
    {
        agent.agentTypeID = index;
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
        if(closestDistance == 99999)
        {
            return null;
        }
        return corpses[indexOfClosest].GetComponentInChildren<EnemyHealth>(true).transform;
    } 

    private void Update() //TODO: Update other enemies to use new system
    {
        switch(chaseMode)
        {
            case ChaseMode.pursue:
                EnemyPursue();
                break;
            case ChaseMode.retreat:
                EnemyRetreat();
                break;
            case ChaseMode.res:
                JunkieChase();
                break;
            default:
                EnemyPursue();
                break;
        }
    }

    private void EnemyPursue()
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

    private void EnemyRetreat()
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

    private void JunkieChase()
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
        agent.SetDestination(targetLocation);
    }
}

