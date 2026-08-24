using UnityEngine;
using System.Collections;

public class PlayerScoreHandler : MonoBehaviour
{
    [SerializeField] bool targetGet = false;
    [SerializeField] private GameObject targetAura;

    [SerializeField] private float points = 0f;
    [SerializeField] private float overallPoints = 0f;

    [SerializeField] private int orbsCollected = 0;
    [SerializeField] private int deliveriesMade = 0;
    [SerializeField] private int enemiesKilled = 0;

    private int orbsCollectedOverall = 0;
    private int deliveriesMadeOverall = 0;
    private int enemiesKilledOverall = 0;

    //Points awarded for doing certain things
    private float deliveryPoints = 10f;
    private float collectPoints = 2f;
    //Tied to the specific type of enemy killed (intensive property)
    //[SerializeField] private float enemyKillPoints = 0f; 

    private float deliveryMultiplier = 1f;
    private float collectMultiplier = 1f;
    private float enemyKillMultiplier = 1f;
    private float meterMultiplierCollect = 1f;
    private float meterMultiplierKill = 1f;

    [SerializeField] RestraintMeter restraintMeterScript;


<<<<<<< Updated upstream
=======

>>>>>>> Stashed changes
    private void Awake()
    {
        restraintMeterScript = GetComponent<RestraintMeter>();
        targetGet = false;
    }

    private void Start()
    {
        ResetPoints();
        ResetCounters();
    }

    public void AddPoints(float pointsToAdd)
    {
         
        points += pointsToAdd;
        overallPoints += pointsToAdd;
        restraintMeterScript.AddRestraint((int)(pointsToAdd/2));
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

    public void SetMeterMultiplierCollect(float newMultiplier)
    {
        meterMultiplierCollect = newMultiplier;
    }

    public void SetMeterMultiplierKill(float newMultiplier)
    {
        meterMultiplierKill = newMultiplier;
    }

    private void OnTriggerEnter(Collider other)
    {
        //Collectibles (Collectables?)
        if (other.CompareTag("Collect"))
        {
            orbsCollected++;
            orbsCollectedOverall++;
            Destroy(other.gameObject);
            AddPoints(collectPoints * collectMultiplier);
        }

<<<<<<< Updated upstream
=======
        if (other.CompareTag("Timed Collect"))
        {
            orbsCollected++;
            orbsCollectedOverall++;
            StartCoroutine(ReactivateOrb(other.gameObject, 10f));
            AddPoints((collectPoints-1) * collectMultiplier);
        }

>>>>>>> Stashed changes
        if (other.CompareTag("Big Collect"))
        {
            orbsCollected++;
            orbsCollectedOverall++;
            Destroy(other.gameObject);
<<<<<<< Updated upstream
            AddPoints(4f * collectPoints * collectMultiplier);
=======
            AddPoints(8f * collectPoints * collectMultiplier);
>>>>>>> Stashed changes
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

    private IEnumerator ReactivateOrb(GameObject orb, float waitTime)
    {
        orb.SetActive(false);
        yield return new WaitForSeconds(waitTime);
        orb.SetActive(true);
    }
}
