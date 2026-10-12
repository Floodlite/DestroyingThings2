using UnityEngine;

public class SparkMove : MonoBehaviour
{
<<<<<<< Updated upstream
    private Player[] players;
    [SerializeField] private float travelSpeed;
    private float distanceToPlayer;
    private float waitTime;


    private Player FindClosestPlayer()
    {
        players = FindObjectsByType<Player>(FindObjectsSortMode.InstanceID);
        if (players == null || players.Length == 0)
=======
    [SerializeField] private GameObject boundEnemy;
    [SerializeField] private float timeElapsed = 0f;
    [SerializeField] private float waitTime = 3f;
    [SerializeField] private float travelSpeed = 7f;
    [SerializeField] private bool bondedToEnemy = false;
    [SerializeField] private Vector3 enemyLocation;
    private Reanimator[] enemies;
    private float distanceToEnemy;


    private void LateUpdate()
    {
        if(boundEnemy != null)
        {
            enemyLocation = boundEnemy.transform.position;
        }

        if(bondedToEnemy) {
            this.transform.position = boundEnemy.gameObject.transform.position;
        }
        timeElapsed += Time.deltaTime;
        if(timeElapsed > 0f && timeElapsed % waitTime == 0f) //Move to new enemy
        {
            bondedToEnemy = false;
            boundEnemy = FindClosestEnemy().transform.parent.gameObject;
            //Physically move to new enemy
        }
    }

    private void MoveToEnemy()
    {
        
    }

    private Reanimator FindClosestEnemy()
    {
        enemies = FindObjectsByType<Reanimator>(FindObjectsSortMode.InstanceID);
        if (enemies == null || enemies.Length == 0)
>>>>>>> Stashed changes
        {
            return null;
        }

        float closestDistance = 99999;
        int indexOfClosest = 0;

<<<<<<< Updated upstream
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
=======
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
>>>>>>> Stashed changes
    }




}