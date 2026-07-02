using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 8;
    [SerializeField] private int health = 8;
    [SerializeField] private bool iFrames = false;
    private float iFramesUptime = 0.2f;
    [SerializeField] private PlayerScoreHandler scoreHandler;
    [SerializeField] private RestraintMeter restraintMeterScript;

    private void Awake()
    {
        scoreHandler = GetComponent<PlayerScoreHandler>();
        restraintMeterScript = GetComponent<RestraintMeter>();
    }

    private void Start()
    {
        iFrames = false;
        ResetHP();
    }

    private void ResetHP()
    {
        health = maxHealth;
    }

    public void LoseHP(int healthLoss)
    {
        if(iFrames) { return; }
        health -= healthLoss;
        Debug.Log("Player " + health);
        if (health <= 0)
        {
            Death();
            return;
        }
        if(restraintMeterScript != null) { restraintMeterScript.AddRestraintClamped(healthLoss/2); }
        StartCoroutine(IFrames());
    }

    public void GainHP(int healthGain)
    {
        if (health + healthGain > maxHealth)
        {
            health = maxHealth;
        }
        else
        {
            health += healthGain;
        }
        Debug.Log("Player " + health);
    }

    private void Death()
    {
        Debug.Log("Failure failure failure");
        if(scoreHandler != null)
        {
            scoreHandler.MultiplyPoints(0.8f);
        }
        Destroy(gameObject);
    }

    private IEnumerator IFrames()
    {
        iFrames = true;
        PulseColorOn();
        yield return new WaitForSeconds(iFramesUptime);
        iFrames = false;
        PulseColorOff();
    }

    private void PulseColorOn()
    {
        foreach (SkinnedMeshRenderer renderer in GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", Color.white);
        }
    }

    private void PulseColorOff()
    {
        foreach (SkinnedMeshRenderer renderer in GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            renderer.material.DisableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", Color.black);
        }
    }
}
