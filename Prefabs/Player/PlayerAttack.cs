using UnityEngine;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject hurtBox;
    [SerializeField] private MeshRenderer mr;
    [SerializeField] private BoxCollider bc;
    [SerializeField] private bool punchInProgress = false;
    private float boxSize = 0.2f;



    public void BringTheHurt()
    {
        punchInProgress = !punchInProgress;
        hurtBox.gameObject.SetActive(punchInProgress);
    }

    public void BringTheHurtII(float punchUptime, float punchDimensions, bool longPunch)
    {
        //Full punch cycle
        //Attack button pressed
        //Shrink punch box to 0.2 on all sides, 
        //enable mesh renderer, 
        //grow to full size, 
        //enable box collider,
        //wait 1 second,
        //shrink box back down and disable the enabled components

        if (!punchInProgress) {
            StartCoroutine(PunchCycle(punchUptime, punchDimensions, longPunch));
        }
    }
    
    
    IEnumerator PunchCycle(float punchDuration, float punchSize, bool longPunch)
    {
        punchInProgress = true;
        mr.enabled = false;
        bc.isTrigger = true;
        bc.enabled = false;
        boxSize = 0.2f;

        hurtBox.transform.localScale = new Vector3(boxSize, boxSize, boxSize);
        yield return new WaitForSeconds(0.1f);
        mr.enabled = true;

        while (boxSize < punchSize)
        {
            boxSize += 0.1f;
            hurtBox.transform.localScale = new Vector3(boxSize, boxSize, boxSize);
            yield return new WaitForSeconds(0.005f);
        }

        bc.enabled = true;
        /*hurtBox.transform.localPosition += new Vector3(0f, 0f, 4f);
        if(longPunch)
        {
            for(int i=0; i<=3; i++) {
                yield return new WaitForSeconds(0.2f);
                hurtBox.transform.localPosition += new Vector3(0f, 0f, 2f);
            }
        }*/

        yield return new WaitForSeconds(punchDuration);

        while (boxSize > 0.2f)
        {
            boxSize -= 0.1f;
            hurtBox.transform.localScale = new Vector3(boxSize, boxSize, boxSize);
            yield return new WaitForSeconds(0.01f);
        }
        hurtBox.transform.localPosition = new Vector3(0f, 1.5f, 2f);

        
        mr.enabled = false;
        bc.enabled = false;
        punchInProgress = false;
    }
}
