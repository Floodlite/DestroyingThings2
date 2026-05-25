using UnityEngine;

public class PlayerScoreHandler : MonoBehaviour
{
    [SerializeField] bool targetGet = false;
    [SerializeField] private GameObject targetAura;

    [SerializeField] private float points = 0f;
    [SerializeField] private float overallPoints = 0;

    [SerializeField] private int orbsCollected = 0;
    [SerializeField] private int deliveriesMade = 0;
    [SerializeField] private int enemiesKilled = 0;

    private int orbsCollectedOverall = 0;
    private int deliveriesMadeOverall = 0;
    private int enemiesKilledOverall = 0;

    //Points awarded for doing certain things
    private float deliveryPoints = 10f;
    private float collectPoints = 1f;
    //Tied to the specific type of enemy killed (intensive property)
    //[SerializeField] private float enemyKillPoints = 0f; 

    private float deliveryMultiplier = 1f;
    private float collectMultiplier = 1f;
    private float enemyKillMultiplier = 1f;



    public void Awake()
    {
        targetGet = false;
        ResetPoints();
        ResetCounters();
    }

    public void AddPoints(float pointsToAdd)
    {
        points += pointsToAdd;
        overallPoints += pointsToAdd;
    }

    public void LosePoints(float pointsToLose)
    {
        points -= pointsToLose;
        overallPoints -= points;
    }

    public void MultiplyPoints(float multiplier)
    {
        points *= multiplier;
    }

    public void ResetPoints()
    {
        points = 0f;
    }

    public void ResetOverallPoints()
    {
        overallPoints = 0f;
    }

    public void ResetCounters()
    {
        orbsCollected = 0;
        deliveriesMade = 0;
        enemiesKilled = 0;
    }

    public void ResetOverallCounters()
    {
        orbsCollectedOverall = 0;
        deliveriesMadeOverall = 0;
        enemiesKilledOverall = 0;
    }

    public float GetKillMultiplier()
    {
        return enemyKillMultiplier;
    }

    public void AddToEnemiesKilled()
    {
        enemiesKilled++;
        enemiesKilledOverall++;
    }

    private void OnTriggerEnter(Collider other)
    {
        //Collectibles
        if (other.CompareTag("Collect"))
        {
            orbsCollected++;
            orbsCollectedOverall++;
            Destroy(other.gameObject);
            AddPoints(collectPoints * collectMultiplier);
        }

        //Deliveries
        if (other.CompareTag("Target"))
        {
            targetGet = true;
            Destroy(other.gameObject);
            Debug.Log("Target acquired");
            targetAura.SetActive(true);
        }
        if (other.CompareTag("Finish"))
        {
            if (targetGet)
            {
                Destroy(other.gameObject);
                Debug.Log("Win");
                targetAura.SetActive(false);
                targetGet = false;

                AddPoints(deliveryPoints * deliveryMultiplier);
                deliveriesMade++;
                deliveriesMadeOverall++;
            }
        }
    }
}
