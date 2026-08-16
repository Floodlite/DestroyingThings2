using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class Racers : MonoBehaviour
{
    //Choose one
    [SerializeField] private Terrain terrain;
    [SerializeField] private GameObject box;
    private enum BoundsType
    {
        terrain,
        box,
    }
    [SerializeField] private BoundsType boundsType = BoundsType.terrain;
    private enum RacerSpeed
    {
        glacial,
        slow,
        med,
        fast,
        hypersonic,
        custom,
    }
    [SerializeField] private RacerSpeed racerSpeed = RacerSpeed.med;
    [SerializeField] private float agentSpeed = 15f;
    [SerializeField] private float turningSpeed = 15f; //Angular speed (pending)
    private float overshootThreshold = 3.5f;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Vector3 targetPoint;


    private void Awake()
    {
        switch(racerSpeed)
        {
            case RacerSpeed.glacial:
                agentSpeed = 17.5f;
                break;
            case RacerSpeed.slow:
                agentSpeed = 25;
                break;
            case RacerSpeed.med:
                agentSpeed = 50;
                break;
            case RacerSpeed.fast:
                agentSpeed = 100;
                break;
            case RacerSpeed.hypersonic:
                agentSpeed = 200;
                break;
            case RacerSpeed.custom:
                agentSpeed = Mathf.Clamp(agentSpeed, 0f, 1000f);
                break;
            default:
                agentSpeed = 100f;
                break;
        }

        agent = this.GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        agent.speed = agentSpeed;
        ChoosePoint();
        GoSomewhere();
    }


    private void Update()
    {
        if (!agent.enabled || agent.pathPending)
        {
            return;
        }

        bool reachedTarget = !agent.hasPath || agent.remainingDistance <= agent.stoppingDistance + overshootThreshold;
        bool targetIsInvalid = !ValidPath(targetPoint);

        if (reachedTarget || targetIsInvalid)
        {
            ChoosePoint();
            GoSomewhere();
        }
    }

    private void GoSomewhere()
    {
        if (targetPoint == Vector3.zero)
        {
            return;
        }

        agent.SetDestination(targetPoint);
    }

    private bool ValidPath(Vector3 destination)
    {
        if (!agent.enabled)
        {
            return false;
        }

        Vector3 startPosition = transform.position;
        if (NavMesh.SamplePosition(startPosition, out NavMeshHit startHit, 1.5f, NavMesh.AllAreas))
        {
            startPosition = startHit.position;
        }

        if (!NavMesh.SamplePosition(destination, out NavMeshHit endHit, 1.5f, NavMesh.AllAreas))
        {
            return false;
        }

        NavMeshPath path = new NavMeshPath();
        if (!NavMesh.CalculatePath(startPosition, endHit.position, NavMesh.AllAreas, path))
        {
            return false;
        }

        return path.status == NavMeshPathStatus.PathComplete && path.corners.Length > 1;
    }


    private void ChoosePoint()
    {
        const int maxAttempts = 20;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector3 candidatePoint;

            if (terrain != null && boundsType == BoundsType.terrain)
            {
                candidatePoint = GetRandomPointOnTerrain();
            }
            else if (box != null && boundsType == BoundsType.box)
            {
                Bounds bounds = GetSpawnBounds();
                candidatePoint = RandomPointInBox(bounds);
            }
            else
            {
                return;
            }

            if (ValidPath(candidatePoint))
            {
                targetPoint = candidatePoint;
                return;
            }
        }

        targetPoint = transform.position;
    }

    private Bounds GetSpawnBounds()
    {
        Collider collider = box.GetComponent<Collider>();
        if (collider != null)
        {
            return collider.bounds;
        }

        Renderer renderer = box.GetComponent<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds;
        }

        return new Bounds(transform.position, transform.localScale);
    }

    private Vector3 RandomPointInBox(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z));
    }

    private Vector3 GetRandomPointOnTerrain()
    {
        Vector3 terrainPos = terrain.transform.position;
        float randomXCoord = Random.Range(terrainPos.x, terrainPos.x + terrain.terrainData.size.x);
        float randomZCoord = Random.Range(terrainPos.z, terrainPos.z + terrain.terrainData.size.z);

        float sampledHeight = terrain.SampleHeight(new Vector3(randomXCoord, 0, randomZCoord));
        float finalYCoord = sampledHeight + terrainPos.y;

        return new Vector3(randomXCoord, finalYCoord, randomZCoord);
    }


    private bool IsOrbOverlapping(Vector3 newPos)
    {
        Collider orbCollider = this.GetComponent<Collider>();
        float overlapRadius = 0.25f;

        if (orbCollider != null)
        {
            overlapRadius = orbCollider.bounds.extents.magnitude;
        }
        else
        {
            Renderer orbRenderer = this.GetComponent<Renderer>();
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

}
