using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Splines;
using Unity.Mathematics;

public class JockeyChase : MonoBehaviour
{
   [SerializeField] private SplineContainer[] splines;
   [SerializeField] private BezierKnot[] splinePoints;
   [SerializeField] private Player[] players;
   [SerializeField] private Player closestPlayer;
   [SerializeField] private SplineContainer chosenSpline;
   [SerializeField] private float splineSpeed = 50f;
   [SerializeField] private Rigidbody rb;
   [SerializeField] private ConstructorConjunction constructors;
   [SerializeField] private bool oppFound;
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
        closestPlayer = FindClosestPlayer();
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
            
            PickRandomSpline();
        }

        Vector3 nextPosition = chosenSpline.EvaluatePosition(distancePercentage + 0.05f);
        Vector3 direction = nextPosition - currentPosition;
        transform.rotation = Quaternion.LookRotation(direction, transform.up);

        if(oppFound) { FlyToPlayer(); }
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
            /*
            Vector3 randomPositionOffset = new Vector3(UnityEngine.Random.Range(-180, 180), 
                UnityEngine.Random.Range(0, 25), 
                UnityEngine.Random.Range(-180, 180));
            */
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
        if(rb == null) { return; }
        rb.MovePosition(this.transform.position + closestPlayer.transform.position * constructors.GetSpeed());
    }
    public void OppFound(bool newStatus)
    {
        oppFound = newStatus;
    }
}
