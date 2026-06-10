using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public class SpawnWave : MonoBehaviour
{
    //Object records the spawn point itself, boolean tracks whether the spot is taken (false --> empty, true --> filled)
    [SerializeField] private Dictionary<GameObject, bool> spawnPoints = new Dictionary<GameObject, bool>();
    [SerializeField] private List<GameObject> enemyPool = new List<GameObject>(); //Assign in inspector
    [SerializeField] private List<GameObject> enemiesOfWave = new List<GameObject>();
    [SerializeField] private List<GameObject> enemiesAlive = new List<GameObject>();
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
        int enemiesInWave = 0;

        while(currentDanger < maxDanger)
        {
            int chosenIndex = Random.Range(0, enemyPool.Count-1);
            GameObject chosenObject = enemyPool[chosenIndex];
            ConstructorConjunction constructors = chosenObject.GetComponentInChildren<ConstructorConjunction>();
            if(constructors == null) { Debug.LogWarning("Constructor Conjunction not found on " + chosenObject);  continue; }
            //EnemyConstructor enemy = constructors.GetEnemy();
            //if(enemy == null) { Debug.LogWarning("Constructors not found on " + chosenObject);  continue; }
            float enemyDangerValue = constructors.GetDangerValue();

            if(currentDanger + enemyDangerValue > maxDanger) { 
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
            Debug.Log("Current Danger: (" + chosenObject + ") " + (currentDanger - enemyDangerValue) + " --> " + currentDanger);

            enemiesInWave++;
            if(enemiesInWave >= maxSpawnPoints)
            {
                break;
            }
        }
        doneChoosing = true;
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
            GameObject spawnedInstance = Pooler.SpawnObject(enemyToSpawn, key.transform.position + new Vector3(0f, 0f, 0f), key.transform.rotation, Pooler.PoolType.waveEnemies);
            if(spawnedInstance != null)
            {
                enemiesOfWave.Add(spawnedInstance);
            }
            spawnPoints[key] = true;
        }
    }

    /*
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
    */

    private bool AllEnemiesDead()
    {
        if(enemiesAlive.Count < 1)
        {   
            Debug.Log("Everyone is dead");
            return true;
        }
        else {
            return false;
        }
    }

    public void AddToEnemiesAlive(GameObject enemyToAdd)
    {
        enemiesAlive.Add(enemyToAdd);
    }

    public void RemoveFromEnemiesAlive(GameObject enemyToRemove)
    {
        enemiesAlive.Remove(enemyToRemove);
    }

    private void ClearWaveEnemies()
    {
        foreach(GameObject enemy in enemiesOfWave) {
            Pooler.ReleaseObjectToPool(enemy, Pooler.PoolType.waveEnemies);
        }
    }

    private void NextWave(float dangerIncrease=2f)
    {
        ClearWaveEnemies();

        //Reset spawn points
        foreach(GameObject key in spawnPoints.Keys.ToList())
        {
            spawnPoints[key] = false;
        }
        
        if(currentWave + 1 > maxWave)
        {
            Debug.Log("All waves cleared!");
        }
        else {
            currentWave++;
            currentDanger = 0f;
            maxDanger += dangerIncrease;
            ChooseEnemies();
        }
    }
}
