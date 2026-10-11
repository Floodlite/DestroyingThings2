using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    //[SerializeField] GameObject conveyorBox; Attach to a gameObject with a box
    [Range(25f, 2000f)]
    [SerializeField] private float pushForce = 10f;
    [SerializeField] private float orbPushDivisor = 5f;

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject == null) { return; }
        Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
        if(rb==null) { rb = other.gameObject.GetComponentInChildren<Rigidbody>(); }
        if(rb==null) { return; }

        if(other.CompareTag("Collect"))
        {
            rb.AddForce(transform.forward*pushForce/orbPushDivisor, ForceMode.Acceleration);
        }
        else
        {
            rb.AddForce(transform.forward*pushForce, ForceMode.Acceleration);
        }
        
    }


}
