using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public class SpawnWave : MonoBehaviour
{
    //Object records the spawn point itself, boolean tracks whether the spot is taken (false --> empty, true --> filled)
    [SerializeField] private Dictionary<GameObject, bool> spawnPoints = new Dictionary<GameObject, bool>();
    [SerializeField] private List<GameObject> enemyPool = new List<GameObject>(); //Assign in inspector
    [SerializeField] private float maxDanger;

    private void Awake()
    {
        spawnPoints = SetSpawns(this.transform, "Spawn");
        if(maxDanger == 0f) { maxDanger = 8f; }
    }

    private void Start()
    {
        Activate();
    }

    public void Activate()
    {
        ChooseEnemies();
    }

    private Dictionary<GameObject, bool> SetSpawns(Transform parent, string tag)
    {
        Dictionary<GameObject, bool> points = new Dictionary<GameObject, bool>();
        Transform[] allTransforms = parent.GetComponentsInChildren<Transform>(true); 

        foreach (Transform childTransform in allTransforms)
        {
            if (childTransform.gameObject != parent.gameObject && childTransform.CompareTag(tag))
            {
                points.Add(childTransform.gameObject, false);
            }
        }
        return points;
    }

    private void ChooseEnemies()
    {
        float currentDanger = 0f;
        bool specialFound = false;
        int maxSpawnPoints = spawnPoints.Count;
        int enemiesSpawned = 0;
        while(currentDanger < maxDanger)
        {
            int chosenIndex = Random.Range(0, enemyPool.Count-1);
            GameObject chosenObject = enemyPool[chosenIndex];
            ConstructorConjunction constructors = chosenObject.GetComponentInChildren<ConstructorConjunction>();
            if(constructors == null) { Debug.LogWarning("Constructor Conjunction not found on " + chosenObject);  continue; }
            //EnemyConstructor enemy = constructors.GetEnemy();
            //if(enemy == null) { Debug.LogWarning("Constructors not found on " + chosenObject);  continue; }
            float enemyDangerValue = constructors.GetDangerValue();

            if(enemyDangerValue + currentDanger > maxDanger) { 
                continue; 
            }
            if(constructors.IsSpecial() && !specialFound) { 
                specialFound = true; 
            }
            else
            {
                continue;
            }

            SpawnEnemy(chosenObject);
            currentDanger += enemyDangerValue;

            enemiesSpawned++;
            if(enemiesSpawned >= maxSpawnPoints)
            {
                break;
            }
        }
    }

    private void SpawnEnemy(GameObject enemyToSpawn)
    {
        //Notice: You cannot modify dictionary values in a standard foreach loop
        foreach(GameObject key in spawnPoints.Keys.ToList())
        {
            //If the spot is already reserved (true), skip to the next one
            if(spawnPoints[key])
            {
                continue;
            }

            //May need to be cleaned up for NavMesh matters
            Instantiate(enemyToSpawn, key.transform.position, key.transform.rotation);
            spawnPoints[key] = true;
        }
    }
}
