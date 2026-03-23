using UnityEngine;
using System.Collections;

public class EnemyActivation : MonoBehaviour
{
    [SerializeField] private float awareness = 0.5f; //how often the enemy will check if the player is coming
    [SerializeField] private float vigilance = 0.5f; //how long the enemy checks if the player is coming
    [SerializeField] private float radius = 4f; //radius of the enemy's detection circle
    [SerializeField] public bool sneakProof = false; //whether or not sneaking within the enemy's detection radius will alert them (unimplemented)
    [SerializeField] private EnemyChase enemyChaseScript;
    [SerializeField] private GameObject detectionSphere;
    private bool stopSearching = false;

    private void Start()
    {
        enemyChaseScript.enabled = false;
        //InvokeRepeating(Detect(radius), 0.5f, awareness);
        StartCoroutine(Detect(radius, awareness, vigilance));
    }

    IEnumerator Detect(float radius, float awareness, float vigilance)
    {
        //create sphere-cast with line of sight
        //if the player is found within, activate the enemy chase scrupt
        //else, keep waiting
    
        yield return new WaitForSeconds(0.5f);
        detectionSphere.SetActive(false);
        detectionSphere.transform.localScale = new Vector3(radius, radius, radius);
        while(true) {
	    if(!stopSearching) {
                yield return new WaitForSeconds(awareness);
                detectionSphere.SetActive(true);
                yield return new WaitForSeconds(vigilance);
                detectionSphere.SetActive(false);
	    }
        }
    }

    public void ActivateChase(bool mode)
    {
        enemyChaseScript.enabled = mode;
	stopSearching = true;
    }


}
