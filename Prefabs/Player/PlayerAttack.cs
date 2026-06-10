using UnityEngine;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private GameObject hurtBox;
    [SerializeField] private MeshRenderer mr;
    [SerializeField] private BoxCollider bc;
    [SerializeField] private bool punchInProgress = false;
    private Player player;
    private float boxSize = 0.2f;
    private float minBoxSize = 0.2f;
    private int punchCount = 1;

    [SerializeField] private RestraintMeter restraintMeterScript;

    private void Awake()
    {
        player = GetComponent<Player>();
        restraintMeterScript = GetComponent<RestraintMeter>();
    }

    private void Start()
    {
        punchCount = 1;
    }

    public void BringTheHurt()
    {
        punchInProgress = !punchInProgress;
        hurtBox.gameObject.SetActive(punchInProgress);
    }

    public void BringTheHurtII(float punchUptime, float punchDimensions)
    {
        /*
        * Full punch cycle:
        * Attack button pressed
        * Shrink punch box to 0.2 on all sides, 
        * enable mesh renderer, 
        * grow to full size, 
        * enable box collider,
        * wait 1 second,
        * shrink box back down and disable the enabled components
        */

        if(restraintMeterScript.GetTurboStatus()) {
            //Debug.Log("Turbo valid");
            int pulverizeCount = restraintMeterScript.GetCurrentRestraint() / restraintMeterScript.GetTurboPunchCost(); 

            if(pulverizeCount > 0 && restraintMeterScript.SpendRestraint(pulverizeCount * restraintMeterScript.GetTurboPunchCost()))
            {
                punchCount = pulverizeCount;
                punchDimensions *= 1 + 0.05f * punchCount;
                //punchUptime = Mathf.Clamp(1.05f*punchUptime/punchCount, 0.01f, 1f);
                //Debug.Log("New uptime: " + punchUptime);
            }
        }

        if(punchCount < 1) {
            punchCount = 1;
        }

        if (!punchInProgress) {
            StartCoroutine(PunchCycle(punchUptime, punchDimensions, punchCount));
        }
        punchCount = 1;
    }
    
    
    IEnumerator PunchCycle(float punchDuration, float punchSize, int punchCount)
    {
        punchInProgress = true;

        for(int i=0; i<punchCount; i++) {
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
                    yield return new WaitForSeconds(0.005f / (punchCount*1.05f));
                }
            
        

            bc.enabled = true;
            yield return new WaitForSeconds(punchDuration);

            
                while (boxSize > minBoxSize)
                {
                    boxSize -= 0.1f;
                    hurtBox.transform.localScale = new Vector3(boxSize, boxSize, boxSize);
                    yield return new WaitForSeconds(0.01f / (punchCount*1.05f));
                }
            
            hurtBox.transform.localPosition = new Vector3(0f, 1.5f, 2f);

            
            mr.enabled = false;
            bc.enabled = false;
        }
        punchInProgress = false;
    }
}
