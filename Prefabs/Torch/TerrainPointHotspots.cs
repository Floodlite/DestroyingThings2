using UnityEngine;
using System.Collections.Generic;

public class TerrainPointHotspots : MonoBehaviour
{
    [SerializeField] private Terrain terrain;
    [SerializeField] private GameObject orb;
    [SerializeField] private int count = 8;
    [SerializeField] private float threshold = 3f;
    [SerializeField] private List<Vector3> orbLocations = new List<Vector3>();

    private void Awake()
    {
        terrain = this.GetComponent<Terrain>();
        CalculateThreshold();
    }

    private void Start()
    {
        PopulateWithOrbs();
    }

    private void PopulateWithOrbs()
    {
        orbLocations.Clear();

        int attemptsPerOrb = Mathf.Max(100, count * 50);
        for (int i = 0; i < count; ++i /*Fancy*/)
        {
            Vector3 newPos = GetRandomPointOnTerrain();
            int attempts = 0;

            while (attempts < attemptsPerOrb && (IsOrbTooClose(newPos) || IsOrbOverlapping(newPos)))
            {
                newPos = GetRandomPointOnTerrain();
                attempts++;
            }

            if (attempts >= attemptsPerOrb && (IsOrbTooClose(newPos) || IsOrbOverlapping(newPos)))
            {
                Debug.Log("Give up.");
                break;
            }

            Pooler.SpawnObject(orb, newPos, transform.rotation, Pooler.PoolType.orbs);
            orbLocations.Add(newPos);
        }
    }

    private Vector3 GetRandomPointOnTerrain()
    {
        Vector3 terrainPos = terrain.transform.position;
        float randomXCoord = Random.Range(terrainPos.x, terrainPos.x + terrain.terrainData.size.x);
        float randomZCoord = Random.Range(terrainPos.z, terrainPos.z + terrain.terrainData.size.z);

        float sampledHeight = terrain.SampleHeight(new Vector3(randomXCoord, 0, randomZCoord));
        float finalYCoord = sampledHeight + terrainPos.y;

        float offset = orb.transform.localScale.y / 2f;
        return new Vector3(randomXCoord, finalYCoord + offset, randomZCoord);
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
        return false;
    }

    private void CalculateThreshold()
    {
        float sizeScale = terrain
            ? Mathf.Max(transform.localScale.x, transform.localScale.z)
            : Mathf.Max(transform.localScale.x, transform.localScale.y, transform.localScale.z);

        float autoThreshold = Mathf.Max(0.1f, sizeScale / Mathf.Max(count, 1));
        threshold = Mathf.Max(threshold, autoThreshold);
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
    
}
