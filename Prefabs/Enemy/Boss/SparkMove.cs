using UnityEngine;

public class SparkMove : MonoBehaviour
{
    [SerializeField] private GameObject boundEnemy;
    [SerializeField] private float timeElapsed = 0f;
    [SerializeField] private float waitTime = 3f;
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private bool boundToEnemy = false;
    [SerializeField] private Vector3 enemyLocation;
    private Reanimator[] enemies;
    private float distanceToEnemy;

    private void FixedUpdate()
    {
        if(boundEnemy != null)
        {
            enemyLocation = boundEnemy.transform.position;
        }

        if(boundToEnemy) {
            this.transform.position = boundEnemy.gameObject.transform.position;
        }
        timeElapsed += Time.deltaTime;
        if(timeElapsed > 0f && timeElapsed % waitTime == 0f) //Move to new enemy
        {
            boundToEnemy = false;
            //boundEnemy = FindClosestEnemy().transform.parent.gameObject;
            boundEnemy = ChooseRandomEnemy().transform.parent.gameObject;

            //Physically move to new enemy
            MoveToEnemy();
        }
    }

    private void MoveToEnemy()
    {
        while(Mathf.Abs(transform.position.sqrMagnitude - boundEnemy.transform.position.sqrMagnitude) >= 2f)
        {
            this.transform.Translate(transform.position-boundEnemy.transform.position * moveSpeed);
        }
    }

    private Reanimator FindClosestEnemy()
    {
        enemies = FindObjectsByType<Reanimator>(FindObjectsSortMode.InstanceID);
        if (enemies == null || enemies.Length == 0)
        {
            return null;
        }

        float closestDistance = 99999;
        int indexOfClosest = 0;

        for(int i=0; i<enemies.Length; i++)
        {
            if(enemies[i].IsDead())
            {
                continue;
            }

            distanceToEnemy = Vector3.Distance(transform.position, enemies[i].transform.position);
            
            if(distanceToEnemy < closestDistance)
            {
                closestDistance = distanceToEnemy;
                indexOfClosest = i;
            }
        }
        return enemies[indexOfClosest];
    }

    private Reanimator ChooseRandomEnemy()
    {
        int randy;
        int maxAttempts = enemies.Length * 4;
        for(int i=0; i<maxAttempts; i++) {
            randy = UnityEngine.Random.Range(0, enemies.Length);
            if(enemies[randy].GetComponent<SparkMark>() == null)
            {
                enemies[randy].transform.gameObject.AddComponent<SparkMark>(); 
                return enemies[randy];
            }
        }
        return null;
    }




}