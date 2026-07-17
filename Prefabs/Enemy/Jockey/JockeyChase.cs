using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Collections;

public class JockeyChase : MonoBehaviour
{
   [SerializeField] private GameObject[] splines;
   [SerializeField] private BezierKnot[] splinePoints;
   [SerializeField] private Player[] players;
   [SerializeField] private Player closestPlayer;
   [SerializeField] private GameObject chosenSpline;
   [SerializeField] private GameObject currentSplineTemplate;
   [SerializeField] private SplineContainer chosenSplineContainer;
   [SerializeField] private float splineSpeed = 50f;
   [SerializeField] private Rigidbody rb;
   [SerializeField] private ConstructorConjunction constructors;
   [SerializeField] private bool oppFound = false;
   [SerializeField] private float distancePercentage = 0f;
   [SerializeField] private int attempts = 0;
   private float splineLength;
   [SerializeField] private int splineLapCount = 0; //Tracks how many times the enemy has made a complete loop around the spline

   //How much the new flight path will be nudged in the player's direction
   [SerializeField] private float playerEncroachmentFactor = 0.5f;
   [SerializeField] private FirecrackerFire fFire;
   [SerializeField] private bool stunned = false;
   [SerializeField] private float stunDuration = 5f;
   [SerializeField] private float flightHeight = 5f; //Fun to say



    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        constructors = GetComponent<ConstructorConjunction>();
        players = FindObjectsByType<Player>(FindObjectsSortMode.None);
        fFire = GetComponent<FirecrackerFire>();
        fFire.enabled = false;
    }

    private void Start()
    {
        closestPlayer = FindClosestPlayer();
        InitializeSpline();
        flightHeight = FindFlightHeight();
    }

    private float FindFlightHeight() {
        if (Physics.Raycast(this.transform.position, -transform.up, out RaycastHit hit, Mathf.Infinity))
        { 
            return Mathf.Abs(hit.transform.position.y - this.transform.position.y);
        }
        return 5f;
    }

    private void InitializeSpline()
    {
        if (splines == null || splines.Length == 0)
        {
            return;
        }

        GameObject initialTemplate = splines[0];
        currentSplineTemplate = initialTemplate;
        chosenSpline = initialTemplate;
        chosenSplineContainer = initialTemplate.GetComponent<SplineContainer>();

        if (chosenSplineContainer == null)
        {
            return;
        }

        distancePercentage = 0f;
        CaclulateSplineLength(chosenSplineContainer);
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
        if(stunned) { return; }

        closestPlayer = FindClosestPlayer();
        if(oppFound) { FlyToPlayer(); return; }
            //fFire.enabled = true; Save for Absolute varianct


        if (chosenSpline == null)
        {
            return;
        }

        chosenSplineContainer = chosenSpline.GetComponent<SplineContainer>();
        if(chosenSplineContainer == null) { return; }

        distancePercentage += splineSpeed * Time.deltaTime / splineLength;

        Vector3 currentPosition = chosenSplineContainer.EvaluatePosition(distancePercentage);
        transform.position = currentPosition;

        if (distancePercentage > 1f)
        {
            distancePercentage = 0f;
            splineLapCount++;

            //Bro... All I had to do to solve the problem was move this singular conditional <i>inside</i> the above if-statement instead of outside of it.
            if(splineLapCount > 0 && splineLapCount % 4 == 0)
            {
                UpdateEncroachmentFactor(0.055f);
                PickRandomSpline();
            }
        }
        

        Vector3 nextPosition = chosenSplineContainer.EvaluatePosition(distancePercentage + 0.05f);
        Vector3 direction = nextPosition - currentPosition;
        transform.rotation = Quaternion.LookRotation(direction, transform.up);
    }

    private void PickRandomSpline()
    {
        if (splines == null || splines.Length == 0)
        {
            return;
        }

        GameObject nextSplineTemplate = SelectDifferentSplineTemplate();
        if (nextSplineTemplate == null)
        {
            return;
        }

        SpawnSpline(nextSplineTemplate);
        if (chosenSplineContainer != null)
        {
            CaclulateSplineLength(chosenSplineContainer);
        }
    }

    private GameObject SelectDifferentSplineTemplate()
    {
        if (splines == null || splines.Length == 0)
        {
            return null;
        }

        for (int i = 0; i < splines.Length * 2; i++)
        {
            GameObject splineCandidate = splines[UnityEngine.Random.Range(0, splines.Length)];
            if (splineCandidate != currentSplineTemplate)
            {
                return splineCandidate;
            }
        }

        return splines[0];
    }

    private void SpawnSpline(GameObject templateSpline)
    {
        if (templateSpline == null)
        {
            return;
        }

        SplineContainer templateSplineContainer = templateSpline.GetComponent<SplineContainer>();
        if (templateSplineContainer == null)
        {
            return;
        }

        attempts = 0;
        int maxAttempts = 12;
        GameObject validSpawn = null;
        SplineContainer validContainer = null;

        while (attempts < maxAttempts)
        {
            bool splineValid = true;
            Vector3 directionToPlayer;
            if (closestPlayer != null)
            {
                Vector3 toPlayer = closestPlayer.transform.position - transform.position;
                directionToPlayer = toPlayer * Mathf.Clamp01(playerEncroachmentFactor);
            }
            else
            {
                directionToPlayer = Vector3.zero;
            }

            Vector3 randomRotationOffset = new Vector3(UnityEngine.Random.Range(-180, 180), 
                UnityEngine.Random.Range(-180, 180), 
                UnityEngine.Random.Range(-180, 180));
            Vector3 randomScaleMultiplier = new Vector3(UnityEngine.Random.Range(10, 21) / 10f,
                UnityEngine.Random.Range(10, 21) / 10f,
                UnityEngine.Random.Range(10, 21) / 10f);
            Vector3 randomPositionOffset = new Vector3(UnityEngine.Random.Range(-10f, 10f),
                UnityEngine.Random.Range(-2f, 2f),
                UnityEngine.Random.Range(-10f, 10f));

            GameObject objSpline = templateSpline.gameObject;
            Vector3 spawnPosition = objSpline.transform.position + directionToPlayer + randomPositionOffset;
            Quaternion spawnRotation = objSpline.transform.rotation * Quaternion.Euler(randomRotationOffset);
            GameObject spawnedSpline = Pooler.SpawnObject(objSpline, spawnPosition, spawnRotation, Pooler.PoolType.splines);

            if (spawnedSpline != null)
            {
                Vector3 baseScale = Vector3.one;
                Vector3 newScale = new Vector3(
                    Mathf.Clamp(baseScale.x * randomScaleMultiplier.x, 0.25f, 4f),
                    Mathf.Clamp(baseScale.y * randomScaleMultiplier.y, 0.25f, 4f),
                    Mathf.Clamp(baseScale.z * randomScaleMultiplier.z, 0.25f, 4f));
                spawnedSpline.transform.localScale = newScale;
            }

            if (spawnedSpline != null)
            {
                foreach (Transform sphereChildTransform in spawnedSpline.transform)
                {
                    Vector3 samplePosition = spawnedSpline.transform.TransformPoint(sphereChildTransform.localPosition);
                    if (IsOrbOverlapping(samplePosition, sphereChildTransform.gameObject, attempts, spawnedSpline.transform))
                    {
                        splineValid = false;
                        Pooler.ReleaseObjectToPool(spawnedSpline, Pooler.PoolType.splines);
                        break;
                    }
                }
            }

            if (splineValid)
            {
                validSpawn = spawnedSpline;
                validContainer = spawnedSpline != null ? spawnedSpline.GetComponent<SplineContainer>() : templateSplineContainer;
                break;
            }

            attempts++;
        }

        if (validSpawn != null)
        {
            currentSplineTemplate = templateSpline;
            chosenSpline = validSpawn;
            chosenSplineContainer = validContainer;
            distancePercentage = 0f;
            CaclulateSplineLength(chosenSplineContainer);
            return;
        }

        currentSplineTemplate = templateSpline;
        chosenSpline = templateSpline;
        chosenSplineContainer = templateSplineContainer;
        distancePercentage = 0f;
        CaclulateSplineLength(chosenSplineContainer);
        //Debug.LogWarning("Unable to find a valid spline placement after " + maxAttempts + " attempts. Falling back to the template spline.");
        //Debug.LogWarning("Give up."); Never give up.
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

    private bool IsOrbOverlapping(Vector3 newPos, GameObject orb, int attemptsElapsed, Transform ignoredTransform = null)
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

            if(attemptsElapsed > 6) {
                overlapRadius /= 4f;
            }
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

            if (ignoredTransform != null && (hitCollider.transform == ignoredTransform || hitCollider.transform.IsChildOf(ignoredTransform)))
            {
                continue;
            }

            if (hitCollider is TerrainCollider)
            {
                continue;
            }

            string layerName = LayerMask.LayerToName(hitCollider.gameObject.layer);
            if (layerName == "Ground" || layerName == "Terrain" || layerName == "Environment" || layerName == "Floor")
            {
                continue;
            }

            return true;
        }
        return false;
    }

    public void UpdateEncroachmentFactor(float toAdd)
    {
        playerEncroachmentFactor = Mathf.Clamp(playerEncroachmentFactor+toAdd, 0.5f, 1f);
    }

    public void FlyToPlayer()
    {
        if(rb == null || closestPlayer == null) { return; }
        rb.useGravity = true;
        //rb.MovePosition(this.transform.position + closestPlayer.transform.position * constructors.GetSpeed());
        Vector3 directionToPlayer = closestPlayer.transform.position - transform.position;
        //rb.linearVelocity = directionToPlayer * constructors.GetSpeed();
        Vector3 direction = directionToPlayer.normalized;
        //rb.MovePosition(transform.position + direction * constructors.GetSpeed() * Time.deltaTime);
        transform.rotation = Quaternion.LookRotation(direction, transform.up);
        rb.AddForce(direction * constructors.GetSpeed() * 4f, ForceMode.Impulse);
        StartCoroutine(Stunned());
    }

    private IEnumerator Stunned() {
        stunned = true;
        yield return new WaitForSeconds(stunDuration);
        rb.useGravity = false;
        Vector3 returnPosition = new Vector3(transform.position.x, transform.position.y + flightHeight, transform.position.z);
        rb.linearVelocity = Vector3.zero; //Clear residual fall velocity so it doesn't keep dropping post-teleport
        rb.MovePosition(returnPosition);
        stunned = false;
    }

    public void OppFound(bool newStatus)
    {
        oppFound = newStatus;
    }
}
