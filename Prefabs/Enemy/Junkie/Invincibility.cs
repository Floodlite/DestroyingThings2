using UnityEngine;
using System.Collections;

public class Invincibility : MonoBehaviour
{
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private float invincibilityDuration = 50f;
    [SerializeField] private GameObject shield;
    [SerializeField] private MeshRenderer shieldRenderer;


    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        shieldRenderer = shield.GetComponent<MeshRenderer>();
    }

    private void OnEnable()
    {
        StartCoroutine(StartCountdown());
    }

    private IEnumerator StartCountdown()
    {
        shieldRenderer.enabled = true;
        enemyHealth.SetInvincibility(true);
        yield return new WaitForSeconds(invincibilityDuration);
        enemyHealth.SetInvincibility(false);
        shieldRenderer.enabled = false;
    }
}
