using UnityEngine;
using System.Collections;

public class TrampolineBounce : MonoBehaviour
{
    private enum BounceStrength
    {
        low,
        med,
        high,
        custom
    }
    [SerializeField] private BounceStrength bounceStrength = BounceStrength.med;
    [SerializeField] private float bounceForce = 5f;
    [SerializeField] private GameObject rim;
    private Material material;

    private void Awake()
    {
        if(rim == null)
        {
            return;
        }
        material = rim.GetComponentInParent<MeshRenderer>().material;
    }

    private void Start()
    {
        switch(bounceStrength)
        {
            case BounceStrength.low:
                bounceForce = 10f;
                material.color = new Color(34/255f, 139/255f, 35/255f);
                break;
            case BounceStrength.med:
                bounceForce = 30;
                material.color = new Color(0f, 0f, 1f);
                break;
            case BounceStrength.high:
                bounceForce = 55f;
                material.color = new Color(255f, 255f, 255f);
                break;
            case BounceStrength.custom:
                bounceForce = Mathf.Clamp(bounceForce, 5f, 10000f);
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
        if(rb != null)
        {
            rb.AddForce(transform.up * bounceForce, ForceMode.Impulse);
            Debug.Log("Boing");
        }
    }
}
