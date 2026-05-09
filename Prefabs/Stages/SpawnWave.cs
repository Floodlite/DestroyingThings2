using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public class SpawnWave : MonoBehaviour
{
    //Object records the spawn point itself, boolean tracks whether the spot is taken (false --> empty, true --> filled)
    [SerializeField] private Dictionary<GameObject, bool> spawnPoints = new Dictionary<GameObject, bool>();
    [SerializeField] private List<GameObject> enemyPool = new List<GameObject>(); //Assign in inspector
    [SerializeField] Reanimator[] enemiesAlive;
    [SerializeField] private bool everyoneDead = false;
    [SerializeField] private bool doneChoosing = false;

    [SerializeField] private float maxDanger = 8f;
    [SerializeField] private float currentDanger = 0f;

    [SerializeField] private int currentWave = 1;

    [SerializeField] private int maxWave = 8;

    private void Awake()
    {
        spawnPoints = SetSpawns(this.transform, "Spawn");
        if(maxDanger == 0f) { maxDanger = 8f; }
        currentWave = 1;
        doneChoosing = false;
    }

    private void Start()
    {
        ChooseEnemies();
        doneChoosing = false;
    }

    private void LateUpdate()
    {
        everyoneDead = AllEnemiesDead();
        if(everyoneDead && doneChoosing)
        {
            NextWave(2f);
            doneChoosing = false;
        }
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
        currentDanger = 0f;
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
            else if(constructors.IsSpecial() && specialFound)
            {
                continue;
            }

            SpawnEnemy(chosenObject);
            currentDanger += enemyDangerValue;

            enemiesSpawned++;
            if(enemiesSpawned >= maxSpawnPoints)
            {
                doneChoosing = true;
                break;
            }
        }
        return;
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

    private bool AllEnemiesDead()
    {
        //Switch to FindObjectsSortMode.None if performance issues arise
        enemiesAlive = FindObjectsByType<Reanimator>(FindObjectsSortMode.InstanceID);
        foreach(Reanimator script in enemiesAlive)
        {
            if(!script.IsDead()) {
                return false;
            }
            //TODO: Remove enemies from their pool and change the length of the array accordingly
        }
        return true;
    }

    private void NextWave(float dangerIncrease=2f)
    {
        if(currentWave + 1 > maxWave)
        {
            Debug.Log("All waves cleared!");
        }
        else {
            currentWave++;
            maxDanger += dangerIncrease;
            ChooseEnemies();
        }
    }
}
