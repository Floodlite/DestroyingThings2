using UnityEngine;

public class ScoreHandler : MonoBehaviour
{
    [SerializeField] bool targetGet = false;
    [SerializeField] private GameObject targetAura;
    [SerializeField] private GameObject chargeAura;

    [SerializeField] private int points = 0;
    [SerializeField] private static int overallPoints = 0;


    private void OnTriggerEnter(Collider other)
    {
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

                points += 10;
            }
        }
    }
}
