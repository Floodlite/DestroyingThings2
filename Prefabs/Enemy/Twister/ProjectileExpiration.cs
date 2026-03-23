using UnityEngine;
using System.Collections;

public class ProjectileExpiration : MonoBehaviour
{
    [SerializeField] private float projectileLifespan = 3f;

    private void OnEnable()
    {
        StartCoroutine(SelfDestruct(projectileLifespan));
        
    }
    private IEnumerator SelfDestruct(float projectileLifespan)
    {
        yield return new WaitForSeconds(projectileLifespan);
        Pooler.ReleaseObjectToPool(gameObject, Pooler.PoolType.bullets);
    }
}
