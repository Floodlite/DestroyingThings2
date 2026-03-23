using UnityEngine;

public class BubbleAlert : MonoBehaviour
{
    [SerializeField] private EnemyActivation enemyActivationScript;
    [SerializeField] private GameObject enemy;

    private void Start()
    {
        enemyActivationScript = enemy.GetComponent<EnemyActivation>();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Punch") && !enemyActivationScript.sneakProof)
        {
            enemyActivationScript.ActivateChase(true);
        }
    }
}
