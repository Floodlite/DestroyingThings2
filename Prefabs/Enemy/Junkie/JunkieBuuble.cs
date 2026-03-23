using UnityEngine;
using System.Collections;

public class JunkieBubble : MonoBehaviour
{
    private EnemyHealth enemyScript;

    private void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent("EnemyHealth");
        }  
    }

        
}
