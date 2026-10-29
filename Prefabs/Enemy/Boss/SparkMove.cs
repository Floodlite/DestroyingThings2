using UnityEngine;

public class SparkMove : MonoBehaviour
{
    [SerializeField] private GameObject boundEnemy;
    [SerializeField] private float timeElapsed = 0f;
    [SerializeField] private float waitTime = 8f;
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
            this.transform.parent = boundEnemy.transform.parent;
        }
        timeElapsed += Time.deltaTime;
        if(timeElapsed > 0f && timeElapsed >= waitTime) //Move to new enemy
        {
            timeElapsed = 0f;
            boundToEnemy = false;
            //boundEnemy = FindClosestEnemy().transform.parent.gameObject;
            boundEnemy = ChooseRandomEnemy().transform.parent.gameObject;

            //Physically move to new enemy
            MoveToEnemy();
        }
    }

    private void MoveToEnemy()
    {
        while(Mathf.Abs(transform.position.sqrMagnitude - enemyLocation.sqrMagnitude) >= 2f)
        {
            this.transform.Translate((transform.position-enemyLocation).normalized * moveSpeed);
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