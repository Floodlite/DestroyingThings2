using UnityEngine;
using System.Collections;

public class JunkieMagic : MonoBehaviour
{
    [SerializeField] private GameObject enemy;
    [SerializeField] private EnemyHealth enemyScript;
    [SerializeField] private float resSpeed = 2f;
    //private bool sphereHit = false;

    private void Start()
    {
        enemyScript = enemy.GetComponent<EnemyHealth>();
        StartCoroutine(StartRessing(resSpeed));
    }


    IEnumerator StartRessing(float resSpeed)
    {
        while(true) {
            Res();
            yield return new WaitForSeconds(resSpeed);
        }
    }

    private void Res() {
        //sphereHit = Physics.SphereCast(GetComponent<Collider>().bounds.center, 1.5f, Vector3.zero, out objectHit);

    }

     
        
}
