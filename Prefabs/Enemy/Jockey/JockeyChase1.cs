using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Splines;
using Unity.Mathematics;

public class JockeyChase1 : MonoBehaviour
{
   [SerializeField] private GameObject[] splines;
   [SerializeField] private BezierKnot[] splinePoints;
   [SerializeField] private Player[] players;
   [SerializeField] private Player closestPlayer;
   [SerializeField] private GameObject chosenSpline;
   [SerializeField] private SplineContainer chosenSplineContainer;
   [SerializeField] private float splineSpeed = 50f;
   [SerializeField] private Rigidbody rb;
   [SerializeField] private ConstructorConjunction constructors;
   [SerializeField] private bool oppFound = false;
   private float distancePercentage = 0f;
   private float splineLength;
   private int splineLapCount = 0; //Tracks how many times the enemy has made a complete loop around the spline

   //How much the new flight path will be nudged in the player's direction
   private float playerEncroachmentFactor = 0.5f; //1f: Full distance, 0.5f: Halfway to the player


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        constructors = GetComponent<ConstructorConjunction>();
    }

    private void Start()
    {
        closestPlayer = FindClosestPlayer();
        PickRandomSpline();
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

    private void FixedUpdate()
    {
        chosenSplineContainer = chosenSpline.GetComponent<SplineContainer>();
        if(chosenSplineContainer == null) { return; }

        closestPlayer = FindClosestPlayer();
        distancePercentage += splineSpeed * Time.deltaTime / splineLength;

        Vector3 currentPosition = chosenSplineContainer.EvaluatePosition(distancePercentage);
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

        Vector3 nextPosition = chosenSplineContainer.EvaluatePosition(distancePercentage + 0.05f);
        Vector3 direction = nextPosition - currentPosition;
        transform.rotation = Quaternion.LookRotation(direction, transform.up);

        if(oppFound) { FlyToPlayer(); }
    }

    private void PickRandomSpline()
    {
        if (splines == null || splines.Length == 0)
        {
            return;
        }

        chosenSpline = splines[UnityEngine.Random.Range(0, splines.Length)];
        SpawnSpline(chosenSpline);
        CaclulateSplineLength(chosenSplineContainer);
    }

    private void SpawnSpline(GameObject chosenSpline)
    {
        if (chosenSpline == null)
        {
            return;
        }

        chosenSplineContainer = chosenSpline.GetComponent<SplineContainer>();
        if (chosenSplineContainer == null)
        {
            return;
        }

        int attempts = 0;
        int maxAttempts = 10;

        while (attempts < maxAttempts)
        {
            bool splineValid = true;
            /*
            Vector3 randomPositionOffset = new Vector3(UnityEngine.Random.Range(-180, 180), 
                UnityEngine.Random.Range(0, 25), 
                UnityEngine.Random.Range(-180, 180));
            */
            Vector3 directionToPlayer;
            if (closestPlayer != null)
            {
                Vector3 toPlayer = closestPlayer.transform.position - transform.position;
                directionToPlayer = toPlayer.sqrMagnitude > 0.0001f
                    ? toPlayer.normalized * playerEncroachmentFactor
                    : Vector3.forward * playerEncroachmentFactor;
            }
            else
            {
                directionToPlayer = Vector3.one * playerEncroachmentFactor;
            }

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

            if (spawnedSpline != null)
            {   float newX = spawnedSpline.transform.localScale.x*randomScaleMultiplier.x;
                float newY = spawnedSpline.transform.localScale.y*randomScaleMultiplier.y;
                float newZ = spawnedSpline.transform.localScale.z*randomScaleMultiplier.z;
                spawnedSpline.transform.localScale = new Vector3(Mathf.Clamp(newX, -100000000000000000, 100000000000000000),
                    Mathf.Clamp(newY, -100000000000000000, 100000000000000000),
                    Mathf.Clamp(newZ, -100000000000000000, 100000000000000000));
            }

            foreach (Transform sphereChildTransform in chosenSpline.transform)
            {
                //P.S. I'm pretty sure that spline containers do not trigger colliders on their own
                if (IsOrbOverlapping(sphereChildTransform.position, sphereChildTransform.gameObject))
                {
                    splineValid = false;
                    if (spawnedSpline != null)
                    {
                        Pooler.ReleaseObjectToPool(spawnedSpline, Pooler.PoolType.splines);
                    }
                    break;
                }
            }

            if (splineValid)
            {
                return;
            }

            attempts++;
        }

        Debug.LogWarning("Unable to find a valid spline placement after " + maxAttempts + " attempts.");
        Debug.LogWarning("Give up.");
    }


    public void CaclulateSplineLength(SplineContainer spline)
    {
        splineLength = spline.CalculateLength();
    }

    public void ChangeSpline(GameObject newSpline)
    {
        chosenSpline = newSpline;
        chosenSplineContainer = chosenSpline.GetComponent<SplineContainer>();
        distancePercentage = 0f;
        CaclulateSplineLength(chosenSplineContainer);
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
        if(rb == null || closestPlayer == null) { return; }
        rb.MovePosition(this.transform.position + closestPlayer.transform.position * constructors.GetSpeed());
    }

    public void OppFound(bool newStatus)
    {
        oppFound = newStatus;
    }
}
