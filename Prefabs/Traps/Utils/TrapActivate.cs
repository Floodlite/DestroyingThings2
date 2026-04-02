using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrapActivate : MonoBehaviour
{
    [SerializeField] private TrapConstructor trapStats;
    [SerializeField] private List<GameObject> fxChain = new List<GameObject>();

    private void Start()
    {
        fxChain = AddObjectsOfTag(this.transform.parent, "Particles");
    }

    private List<GameObject> AddObjectsOfTag(Transform parent, string tag)
    {
        List<GameObject> objects = new List<GameObject>();

        Transform[] allTransforms = parent.GetComponentsInChildren<Transform>(true); 

        foreach (Transform childTransform in allTransforms)
        {
            if (childTransform.gameObject != parent && childTransform.CompareTag(tag))
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
        if (!other.CompareTag("Enemy"))
        {
            return;
        }
        StartCoroutine(StartDetonation(trapStats.armTime));
    }

    IEnumerator StartDetonation(float armTime)
    {
        TrapHurtBox trapHurtBoxScript = GetComponentInChildren<TrapHurtBox>();

        yield return new WaitForSeconds(armTime);
        trapHurtBoxScript.enabled = true;
        playParticles();
        yield return new WaitForSeconds(armTime * 0.2f);
        trapHurtBoxScript.enabled = true;
        Destroy(gameObject);
    }

    private void playParticles()
    {
        foreach (GameObject fx in fxChain)
        {
            ParticleSystem particles = fx.GetComponentInChildren<ParticleSystem>();
            particles.Play(true);
        }
    }
}
