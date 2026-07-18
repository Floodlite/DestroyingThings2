using UnityEngine;
using System.Collections;

public class ProjectileExpiration : MonoBehaviour
{
    [SerializeField] private float projectileLifespan = 3f;

    public void StartSelfDestruct()
    {
        StartCoroutine(SelfDestruct(projectileLifespan));    
    }

    private IEnumerator SelfDestruct(float projectileLifespan)
    {
        yield return new WaitForSeconds(projectileLifespan);
        Debug.Log("Expired (A)");
        TerminateProjectile();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Wall")) //"Untagged" was too inconsistent
        {
            Debug.Log("Expired (B)");
            TerminateProjectile();
        }
    }

    private void TerminateProjectile()
    {
        Pooler.ReleaseObjectToPool(this.gameObject.transform.parent.gameObject, Pooler.PoolType.bullets);
    }
}
