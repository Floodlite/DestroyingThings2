using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JunkiePulse : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject hurtBox;
    private float attackSpeed = 1f;
    private Coroutine attackRoutine;
    private float startRadius;
    [SerializeField] private float endSizeMultiplier = 6f;


    private void Start()
    {
        hurtBox = this.gameObject;
        //hurtBox = FindHurtBox(transform.parent.gameObject);
        startRadius = hurtBox.transform.localScale.x;
        StartAttackRoutine();
    }

    private GameObject FindHurtBox(GameObject parent)
    {
        Transform[] allTransforms = parent.GetComponentsInChildren<Transform>(true); 

        foreach (Transform childTransform in allTransforms)
        {
            if (childTransform.gameObject != parent && childTransform.CompareTag("Hurt Box"))
            {
                return childTransform.gameObject;
            }
        }
        return null;
    }

   private void OnEnable()
    {
        StartAttackRoutine();
    }

    private void OnDisable()
    {
        StopAttackRoutine();
    }

    
    private void StartAttackRoutine()
    {
        StopAttackRoutine();
        attackRoutine = StartCoroutine(PulseCycle(attackSpeed, startRadius, endSizeMultiplier));
    }

    private void StopAttackRoutine()
    {
        if (attackRoutine == null)
        {
            return;
        }

        StopCoroutine(attackRoutine);
        attackRoutine = null;
    }

    IEnumerator PulseCycle(float punchDuration, float startPunchSize, float targetScale)
    {
        float orbSize = startPunchSize;

        while(true) {
            hurtBox.transform.localScale = new Vector3(startRadius, startRadius, startRadius);
            yield return new WaitForSeconds(0.1f);

            while (orbSize < targetScale)
            {
                orbSize += 0.1f;
                hurtBox.transform.localScale = new Vector3(orbSize, orbSize, orbSize);
                yield return new WaitForSeconds(0.1f);
            }

            yield return new WaitForSeconds(punchDuration);

            while (orbSize > startRadius)
            {
                orbSize -= 0.05f;
                hurtBox.transform.localScale = new Vector3(orbSize, orbSize, orbSize);
                yield return new WaitForSeconds(0.01f);
            }
        }
    }
}
