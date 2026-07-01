using UnityEngine;
using System.Collections.Generic;

public class PointHotspots : MonoBehaviour
{
    [SerializeField] private GameObject orb;
    [SerializeField] private int count = 8;
<<<<<<< Updated upstream
    [SerializeField] private float threshold = 3f; //How far away orbs should be from each other
    [SerializeField] private bool rectangle = true;
    [SerializeField] private List<Vector3> orbLocations = new List<Vector3>();

    private void Awake()
    {
        CalculateThreshold();
=======
    private enum SpawnCounts {
        custom,
        low,
        med,
        medHigh,
        high,
    }
    [SerializeField] private SpawnCounts countSetting = SpawnCounts.low;
    [SerializeField] private float threshold = 3f; //How far away orbs should be from each other
    [SerializeField] private bool rectangle = true;
    [SerializeField] private List<Vector3> orbLocations = new List<Vector3>();
    private float offset;

    private void Awake()
    {
        AdjustCountSettings();
        CalculateThreshold();
        //MakeInvisible();
>>>>>>> Stashed changes
    }

    private void Start()
    {
<<<<<<< Updated upstream
=======
        offset = orb.transform.localScale.y / 2f;
>>>>>>> Stashed changes
        PopulateWithOrbs();
    }

    private void PopulateWithOrbs()
    {
        orbLocations.Clear();

        int attemptsPerOrb = Mathf.Max(100, count * 50);
        for (int i = 0; i < count; ++i /*Fancy*/)
        {
            Vector3 newPos = GetRandomPoint();
            int attempts = 0;

<<<<<<< Updated upstream
            while (attempts < attemptsPerOrb && IsOrbTooClose(newPos))
=======
            while (attempts < attemptsPerOrb && (IsOrbTooClose(newPos) || IsOrbOverlapping(newPos)))
>>>>>>> Stashed changes
            {
                newPos = GetRandomPoint();
                attempts++;
            }

<<<<<<< Updated upstream
            if (attempts >= attemptsPerOrb && IsOrbTooClose(newPos))
=======
            if (attempts >= attemptsPerOrb && (IsOrbTooClose(newPos) || IsOrbOverlapping(newPos)))
>>>>>>> Stashed changes
            {
                Debug.Log("Give up.");
                break;
            }

            Pooler.SpawnObject(orb, newPos, transform.rotation, Pooler.PoolType.orbs);
            orbLocations.Add(newPos);
        }
    }

    private Vector3 GetRandomPoint()
    {
        Bounds bounds = GetSpawnBounds();

        if (rectangle)
        {
            return RandomPointInRectangle(bounds);
        }

        return RandomPointInBox(bounds);
    }

    private Vector3 RandomPointInBox(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
<<<<<<< Updated upstream
            Random.Range(bounds.min.y, bounds.max.y),
=======
            Random.Range(bounds.min.y, bounds.max.y) + offset,
>>>>>>> Stashed changes
            Random.Range(bounds.min.z, bounds.max.z));
    }

    private Vector3 RandomPointInRectangle(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
<<<<<<< Updated upstream
            bounds.center.y,
=======
            bounds.center.y + offset,
>>>>>>> Stashed changes
            Random.Range(bounds.min.z, bounds.max.z));
    }

    private Bounds GetSpawnBounds()
    {
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            return collider.bounds;
        }

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds;
        }

        return new Bounds(transform.position, transform.localScale);
    }

    private bool IsOrbTooClose(Vector3 posToCompare)
    {
        float minSqrDistance = threshold * threshold;
        foreach (Vector3 pos in orbLocations)
        {
            if ((posToCompare - pos).sqrMagnitude < minSqrDistance)
            {
                return true;
            }
        }
<<<<<<< Updated upstream

=======
>>>>>>> Stashed changes
        return false;
    }

    private void CalculateThreshold()
    {
        float sizeScale = rectangle
            ? Mathf.Max(transform.localScale.x, transform.localScale.z)
            : Mathf.Max(transform.localScale.x, transform.localScale.y, transform.localScale.z);

        float autoThreshold = Mathf.Max(0.1f, sizeScale / Mathf.Max(count, 1));
        threshold = Mathf.Max(threshold, autoThreshold);
    }
<<<<<<< Updated upstream
=======

    private void AdjustCountSettings()
    {
        Material material;
        MeshRenderer meshRenderer = this.GetComponent<MeshRenderer>();
        if(meshRenderer != null) {
             material = meshRenderer.material;
        }
        //Idea: Change color depending on density for overview purposes

        switch(countSetting)
        {
            case SpawnCounts.custom:
                break;
            case SpawnCounts.low:
                count = 10;
                break;
            case SpawnCounts.med:
                count = 20;
                break;
            case SpawnCounts.medHigh:
                count = 40;
                break;
            case SpawnCounts.high:
                count = 60;
                break;
            default:
                count = 20;
                break; 
        }
    }

    private void MakeInvisible() {
        MeshRenderer meshRenderer = this.GetComponent<MeshRenderer>();
        if(meshRenderer != null) { meshRenderer.enabled = false; }
    }

    private bool IsOrbOverlapping(Vector3 newPos)
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
>>>>>>> Stashed changes
}
