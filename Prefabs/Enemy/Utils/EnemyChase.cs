using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Splines;
using Unity.Mathematics;

public class EnemyChase : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private GameObject player;
    [SerializeField] private Vector3 playerLocation;
    [SerializeField] private Vector3 targetLocation;
    [SerializeField] private Player[] players;
    [SerializeField] private Reanimator[] corpses;
    [SerializeField] private Player closestPlayer;
    [SerializeField] private float navSampleRadius = 4f;
    [SerializeField] private float retreatDistance = 16f;
    private float randomSpeedMultiplier;
    [SerializeField] private bool retreatMode = false;
    [SerializeField] private ConstructorConjunction constructors;
    private float agentSpeed;
    private int baseBoundsIndex;

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
        baseBoundsIndex = NavMesh.GetAreaFromName("Base Bounds");

        //Switch to FindObjectsSortMode.None if performance issues arise
        players = FindObjectsByType<Player>(FindObjectsSortMode.InstanceID);
    }

    private void Start()
    {
        randomSpeedMultiplier = UnityEngine.Random.Range(10, 16)/10f;
        if(constructors != null) {
            agent.speed = constructors.GetSpeed() * randomSpeedMultiplier;
        }
        else
        {
            agent.speed = 15f * randomSpeedMultiplier;
        }
        agentSpeed = agent.speed;

        SwitchRole(EnemyRole.CHASE);
        PlaceAgentOnNavMesh();
        closestPlayer = FindClosestPlayer();
    }

    public void SwitchRole(EnemyRole role)
    {
        enemyRole = role;
        switch(role)
        {
            case EnemyRole.CHASE:
                agent.areaMask |= ~(1 << baseBoundsIndex); //Enables the corresponding layer
                UpdateAgent(0);
                break;
            case EnemyRole.BASE:
                agent.speed *= 1.1f;
                agentSpeed = agent.speed;
                agent.areaMask &= ~(1 << baseBoundsIndex); //Disables the corresponding layer
                UpdateAgent(2);
                break;
            default:
                agent.areaMask |= ~(1 << baseBoundsIndex);
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
        closestPlayer = FindClosestPlayer();
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
        if(!PlaceAgentOnNavMesh()) { return; }

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
        if(!PlaceAgentOnNavMesh()) { return; }

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
        if(!PlaceAgentOnNavMesh()) { return; }

        if(FindClosestCorpse() == null) {
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

    private bool PlaceAgentOnNavMesh()
    {
        if (agent == null)
        {
            return false;
        }

        if (agent.enabled && agent.isOnNavMesh)
        {
            return true;
        }

        bool wasEnabled = agent.enabled;
        agent.enabled = false;

        if (!NavMesh.SamplePosition(transform.position, out NavMeshHit hit,
                navSampleRadius, NavMesh.AllAreas))
        {
            agent.enabled = wasEnabled;
            return false;
        }

        Vector3 agentPosition = hit.position + Vector3.up * agent.baseOffset;
        transform.position = agentPosition;
        agent.enabled = true;
        return agent.isOnNavMesh && agent.Warp(agentPosition);
    }

    public void WarpToNavMesh()
    {
        agent.Warp(transform.position);
    }


   //Spline stuff below this line
   /*
   [SerializeField] private SplineContainer[] splines;
   [SerializeField] private BezierKnot[] splinePoints;
   [SerializeField] private SplineContainer chosenSpline;
   [SerializeField] private float splineSpeed = 50f;
   private float distancePercentage = 0f;
   private float splineLength;
   private int splineLapCount = 0; //Tracks how many times the enemy has made a complete loop around the spline

   //How much the new flight path will be nudged in the player's direction
   private float playerEncroachmentFactor = 0.5f; //1f: Full distance, 0.5f: Halfway to the player


    private void SplineChase()
    {
        distancePercentage += splineSpeed * Time.deltaTime / splineLength;

        Vector3 currentPosition = chosenSpline.EvaluatePosition(distancePercentage);
        transform.position = currentPosition;

        if (distancePercentage > 1f)
        {
            distancePercentage = 0f;
            splineLapCount++;
        }
        if(splineLapCount > 3)
        {
            UpdateEncroachmentFactor(0.05f);
            PickRandomSpline();
        }

        Vector3 nextPosition = chosenSpline.EvaluatePosition(distancePercentage + 0.05f);
        Vector3 direction = nextPosition - currentPosition;
        transform.rotation = Quaternion.LookRotation(direction, transform.up);
    }

    private void PickRandomSpline()
    {
        chosenSpline = splines[UnityEngine.Random.Range(0, splines.Length-1)];
        SpawnSpline(chosenSpline);
        CaclulateSplineLength(chosenSpline);
    }

    private void SpawnSpline(SplineContainer chosenSpline)
    {
        while(true) {
            bool splineValid = true;

            Vector3 directionToPlayer = (closestPlayer.transform.position - transform.position) * playerEncroachmentFactor;
            Vector3 randomRotationOffset = new Vector3(UnityEngine.Random.Range(-180, 180), 
                UnityEngine.Random.Range(-180, 180), 
                UnityEngine.Random.Range(-180, 180));
            Vector3 randomScaleMultiplier = new Vector3(UnityEngine.Random.Range(10, 20)/10, 
                UnityEngine.Random.Range(10, 20)/10f, 
                UnityEngine.Random.Range(10, 20)/10f);

            GameObject objSpline = chosenSpline.gameObject;
            GameObject spawnedSpline = Pooler.SpawnObject(objSpline, objSpline.transform.position + directionToPlayer, 
                objSpline.transform.rotation * Quaternion.Euler(randomRotationOffset), //Multiply quaternions instead of adding them
                Pooler.PoolType.splines);
            spawnedSpline.transform.localScale = new Vector3(spawnedSpline.transform.localScale.x*randomScaleMultiplier.x,
                spawnedSpline.transform.localScale.y*randomScaleMultiplier.y,
                spawnedSpline.transform.localScale.z*randomScaleMultiplier.z);

            foreach (Transform sphereTransform in chosenSpline.transform)
            {
                //P.S. I'm pretty sure that spline containers do not trigger colliders on their own
                if(IsOrbOverlapping(sphereTransform.position, sphereTransform.gameObject))
                {
                    splineValid = false;
                    Pooler.ReleaseObjectToPool(spawnedSpline, Pooler.PoolType.splines);
                    break;
                }
            }

            if(splineValid) { return; }
        }
    }


    public void CaclulateSplineLength(SplineContainer spline)
    {
        splineLength = spline.CalculateLength();
    }

    public void ChangeSpline(SplineContainer newSpline)
    {
        chosenSpline = newSpline;
        distancePercentage = 0f;
        CaclulateSplineLength(chosenSpline);
    }

    public void ChangeSplineSpeed(float newSpeed)
    {
        splineSpeed = newSpeed;
    }

    private bool IsOrbOverlapping(Vector3 newPos, GameObject orb)
    {
        if (orb == null)
        {
            return false;
        }

        Collider orbCollider = orb.GetComponent<Collider>();
        float overlapRadius = 0.25f;

        if (orbCollider != null)
        {
            overlapRadius = orbCollider.bounds.extents.magnitude;
        }
        else
        {
            Renderer orbRenderer = orb.GetComponent<Renderer>();
            if (orbRenderer != null)
            {
                overlapRadius = orbRenderer.bounds.extents.magnitude;
            }
        }

        Collider[] hitColliders = Physics.OverlapSphere(newPos, overlapRadius);
        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider == null)
            {
                continue;
            }

            if (hitCollider == this.GetComponent<Collider>() || hitCollider.transform == transform || hitCollider.transform.IsChildOf(transform))
            {
                continue;
            }

            return true;
        }
        return false;
    }

    public void UpdateEncroachmentFactor(float toAdd)
    {
        playerEncroachmentFactor = Mathf.Clamp(playerEncroachmentFactor+toAdd, 0.5f, 0.85f);
    }

    public void FlyToPlayer()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if(rb == null) { return; }
        rb.MovePosition(this.transform.position + closestPlayer.transform.position * agentSpeed);
    }
    */
}

