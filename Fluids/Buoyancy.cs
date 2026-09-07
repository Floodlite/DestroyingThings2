using UnityEngine;
using System.Linq;
using System.Collections.Generic;

//https://www.youtube.com/watch?v=eL_zHQEju8s
public class Buoyancy : MonoBehaviour
{
    [SerializeField] private Dictionary<GameObject, Rigidbody> floatingObjects = new Dictionary<GameObject, Rigidbody>();
    [SerializeField] private Dictionary<Rigidbody, int> floaterCounts = new Dictionary<Rigidbody, int>();

    [SerializeField] private float depthBeforeSubmerged = 0.5f;
    [SerializeField] private float displacementAmount = 3f;
    [SerializeField] private int objectCount = 0;

    [SerializeField] private bool bonusGravity = false;

    [SerializeField] private float waterDrag = 0.99f;
    [SerializeField] private float waterAngularDrag = 0.5f;

    //Waves
    [SerializeField] private bool waves = false; //busted
    [SerializeField] private float amplitude = 1f;
    [SerializeField] private float length = 2f;
    [SerializeField] private float speed = 1f;
    [SerializeField] private float offset = 0f;


    private void FixedUpdate()
    {
        offset += Time.deltaTime * speed;

        if(floatingObjects.Count <= 0) { return; }
        foreach(GameObject objectum in floatingObjects.Keys.ToList())
        {
            Transform tform = objectum.transform;
            Rigidbody rb = floatingObjects[objectum];
            if(bonusGravity) { rb.AddForceAtPosition(Physics.gravity / floaterCounts[rb], transform.position, ForceMode.Acceleration); }
            float waveHeight = GetWaveHeight(tform.position.x);
            //if(objectum.transform.position.y < this.gameObject.transform.position.y)
            if(waves && objectum.transform.position.y < waveHeight)
            {
                //Debug.Log("Applying forces");
                float displacementMultiplier = Mathf.Clamp01((waveHeight - tform.position.y) / depthBeforeSubmerged) * displacementAmount;
                rb.AddForceAtPosition(new Vector3(0f, Mathf.Abs(Physics.gravity.y) * displacementMultiplier, 0f), tform.position, ForceMode.Acceleration);
            }

            else if(!waves && objectum.transform.position.y < this.transform.position.y)
            {
                float displacementMultiplier = Mathf.Clamp01(-tform.position.y / depthBeforeSubmerged) * displacementAmount;
                //rb.AddForce(new Vector3(0f, Mathf.Abs(Physics.gravity.y) * displacementMultiplier, 0f), ForceMode.Acceleration);
                rb.AddForceAtPosition(new Vector3(0f, Mathf.Abs(Physics.gravity.y) * displacementMultiplier, 0f), tform.position, ForceMode.Acceleration);
                //AddTorque: Applies rotational force to an object
                rb.AddTorque(displacementMultiplier * -rb.linearVelocity * waterDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
                rb.AddTorque(displacementMultiplier * -rb.angularVelocity * waterAngularDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
            }


        }
    }


    private float GetWaveHeight(float x)
    {
        return amplitude * Mathf.Sin(x / length + offset);
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject obj = other.gameObject;
        if(obj == null) { return; }
        Rigidbody rb = obj.GetComponentInParent<Rigidbody>();
        if(rb == null) { return; }
        
        foreach(GameObject objectum in floatingObjects.Keys.ToList())
        {
            if(objectum == obj) { return; } //No duplicate objects allowed
        }
        floatingObjects.Add(obj, rb);
        if(floaterCounts.ContainsKey(rb))
        {
            floaterCounts[rb]++;
        }
        else
        {
            floaterCounts.Add(rb, 1);
        }
        objectCount++;
    }

    private void OnTriggerExit(Collider other)
    {
        GameObject obj = other.gameObject;
        if(obj == null) { return; }
        Rigidbody rb = obj.GetComponentInParent<Rigidbody>();
        if(rb == null) { return; }
        
        if(floatingObjects.ContainsKey(obj))
        {
            floatingObjects.Remove(obj);
            floaterCounts[rb]--;
            if(floaterCounts[rb] == 0) { floaterCounts.Remove(rb); }
            objectCount--;
        }
    }








}
