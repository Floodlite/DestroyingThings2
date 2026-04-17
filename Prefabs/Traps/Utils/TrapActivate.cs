using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrapActivate : MonoBehaviour
{
    [SerializeField] private TrapConstructor trapStats;
    [SerializeField] private List<GameObject> fxChain = new List<GameObject>();
    private Transform parent;
    private TrapHurtBox trapHurtBoxScript;
    private GameObject hurtBox;
    private Collider trapHurtBoxCollider;
    private float hurtBoxStartRadius;

    private void Start()
    {
        parent = this.transform.parent;
        if(parent == null)
        {
            parent = this.transform;
        }

        fxChain = AddObjectsOfTag(this.transform, "Particles");

        trapHurtBoxScript = GetComponentInChildren<TrapHurtBox>();
        trapHurtBoxScript.enabled = false;

        hurtBox = FindHurtBox(transform);
        trapHurtBoxCollider = hurtBox.GetComponentInChildren<Collider>();
        trapHurtBoxCollider.enabled = false;

        hurtBoxStartRadius = hurtBox.transform.localScale.x;
        ShrinkHurtBox(0.5f);
    }

    private List<GameObject> AddObjectsOfTag(Transform parent, string tag)
    {
        List<GameObject> objects = new List<GameObject>();
        Transform[] allTransforms = parent.GetComponentsInChildren<Transform>(true); 

        foreach (Transform childTransform in allTransforms)
        {
            if (childTransform.gameObject != parent.gameObject && childTransform.CompareTag(tag))
            {
                objects.Add(childTransform.gameObject);
            }
        }
        return objects;
    }

    private GameObject FindHurtBox(Transform parent)
    {
        Transform[] allTransforms = parent.GetComponentsInChildren<Transform>(true); 

        foreach (Transform childTransform in allTransforms)
        {
            if (childTransform.gameObject != parent && (childTransform.CompareTag("Hurt Box") || childTransform.CompareTag("Hurt")))
            {
                return childTransform.gameObject;
            }
        }
        return null;
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log(other.tag);
        if (!other.CompareTag("Player"))
        {
            return;
        }
        //Debug.Log("Someone entered");
        StartCoroutine(StartDetonation(trapStats.armTime));
    }

    IEnumerator StartDetonation(float armTime)
    {
        yield return new WaitForSeconds(0.01f);
        //Debug.Log("Coroutine started");
        TrapHurtBox trapHurtBoxScript = GetComponentInChildren<TrapHurtBox>();
        GameObject hurtBox = FindHurtBox(transform);
        Collider trapHurtBoxCollider = hurtBox.GetComponentInChildren<Collider>();

        StartCoroutine(RegrowHurtBox(armTime));
        yield return new WaitForSeconds(armTime);
        trapHurtBoxScript.enabled = true;
        trapHurtBoxCollider.enabled = true;
        //Debug.Log("Script enabled");
        ApplyExplosionForce();

        PlayParticles();
        //Debug.Log("Waiting for" + this.name);
        yield return new WaitForSeconds(armTime * 0.2f);
        Destroy(gameObject);
        //Debug.Log("Discarded trap: " + this.name);
    }

    private void PlayParticles()
    {
        foreach (GameObject fx in fxChain)
        {
            ParticleSystem particles = fx.GetComponentInChildren<ParticleSystem>();
            particles.Play(true);
            //Debug.Log("Particles played: " + particles.name);
        }
    }

    public void ApplyExplosionForce()
    {
        Vector3 explosionPosition = transform.position;
        float radius = hurtBox.transform.localScale.x * 1.15f;
        Collider[] colliders = Physics.OverlapSphere(explosionPosition, radius, ~0);
        float blastForce = trapStats.explosionForce;

        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.AddExplosionForce(blastForce, explosionPosition, radius, blastForce*1.8f, ForceMode.Impulse);
            }
        }
    }

    private void ShrinkHurtBox(float multiplier)
    {
        hurtBox.transform.localScale = new Vector3(hurtBoxStartRadius*multiplier, 
            hurtBoxStartRadius*multiplier, hurtBoxStartRadius*multiplier);
    }

    IEnumerator RegrowHurtBox(float armTime)
    {
        Vector3 startScale = hurtBox.transform.localScale;
        Vector3 targetScale = new Vector3(hurtBoxStartRadius, hurtBoxStartRadius, hurtBoxStartRadius);
        float timeElapsed = 0f;

        if (armTime <= 0f)
        {
            hurtBox.transform.localScale = targetScale;
            yield break;
        }

        while (timeElapsed < armTime)
        {
            timeElapsed += Time.deltaTime;
            float time = Mathf.Clamp01(timeElapsed / armTime);
            hurtBox.transform.localScale = Vector3.Lerp(startScale, targetScale, time);
            yield return null;
        }

        hurtBox.transform.localScale = targetScale;
    }
}
