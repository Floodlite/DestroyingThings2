using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrapActivate : MonoBehaviour
{
    [SerializeField] private TrapConstructor trapStats;
    [SerializeField] private List<GameObject> fxChain = new List<GameObject>();

    private void Start()
    {
        fxChain = AddObjectsOfTag(this.transform, "Particles");

        TrapHurtBox trapHurtBoxScript = GetComponentInChildren<TrapHurtBox>();
        trapHurtBoxScript.enabled = false;

        GameObject hurtBox = FindHurtBox(transform);
        Collider trapHurtBoxCollider = hurtBox.GetComponentInChildren<Collider>();
        trapHurtBoxCollider.enabled = false;
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
        if (!other.CompareTag("Player"))
        {
            return;
        }
        Debug.Log("Someone entered");
        StartCoroutine(StartDetonation(trapStats.armTime));
    }

    IEnumerator StartDetonation(float armTime)
    {
         yield return new WaitForSeconds(0.01f);
        Debug.Log("Coroutine started");
        TrapHurtBox trapHurtBoxScript = GetComponentInChildren<TrapHurtBox>();
        GameObject hurtBox = FindHurtBox(transform);
        Collider trapHurtBoxCollider = hurtBox.GetComponentInChildren<Collider>();

        yield return new WaitForSeconds(armTime);
        trapHurtBoxScript.enabled = true;
        trapHurtBoxCollider.enabled = true;
        Debug.Log("Script enabled");

        PlayParticles();
        Debug.Log("Waiting");
        yield return new WaitForSeconds(armTime * 0.2f);
        Destroy(gameObject);
        Debug.Log("Discarded trap: " + this.name);
    }

    private void PlayParticles()
    {
        foreach (GameObject fx in fxChain)
        {
            ParticleSystem particles = fx.GetComponentInChildren<ParticleSystem>();
            particles.Play(true);
            Debug.Log("Particles played: " + particles.name);
        }
    }
}
