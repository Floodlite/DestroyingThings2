using UnityEngine;
using System.Collections;

public class Geyser : MonoBehaviour
{
    [SerializeField] private GameObject[] objects;
    [SerializeField] private float timeElapsed = 0f;
    [SerializeField] private float launchFrequency = 1f;
    [SerializeField] private float spawnDuration = 5f;
    private int spawnIndex = 0;

    private void Start()
    {
        timeElapsed = 0f;
    }

    private void Update()
    {
        timeElapsed += Time.deltaTime;
        if(timeElapsed >= 3600f) { timeElapsed = 0f; }
        
        if(timeElapsed % launchFrequency == 0) {
            GameObject chosenObject = objects[spawnIndex];
            chosenObject = Pooler.SpawnObject(chosenObject, transform.position, Quaternion.identity, Pooler.PoolType.environmental);
            StartSelfDestruct(chosenObject);

            spawnIndex++;
            if(spawnIndex >= objects.Length)
            {
                spawnIndex = 0;
            }
        }
    }

    private void StartSelfDestruct(GameObject obj)
    {
        StartCoroutine(SelfDestruct(spawnDuration, obj));    
    }

    private IEnumerator SelfDestruct(float projectileLifespan, GameObject obj)
    {
        yield return new WaitForSeconds(projectileLifespan);
        TerminateProjectile(obj);
    }

    private void TerminateProjectile(GameObject obj)
    {
        Pooler.ReleaseObjectToPool(obj, Pooler.PoolType.environmental);
    }
}
