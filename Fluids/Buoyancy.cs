using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class Buoyancy : MonoBehaviour
{
    //public List<GameObject> floatingObjects = new List<GameObject>();
    public Dictionary<GameObject, Rigidbody> floatingObjects = new Dictionary<GameObject, Rigidbody>();
    //TODO: Change GameObject key type to GameObject[] to include all floaters instead of just the parent

    [SerializeField] private float depthBeforeSubmerged = -0.1f;
    [SerializeField] private float displacementAmount = 3f;
    [SerializeField] private int objectCount = 0;


    private void FixedUpdate()
    {
        if(floatingObjects.Count <= 0) { return; }
        foreach(GameObject objectum in floatingObjects.Keys.ToList())
        {
            Transform tform = objectum.transform;
            float waveHeight = WaterWave.instance.GetWaveHeight(tform.position.x);
            //if(objectum.transform.position.y < this.gameObject.transform.position.y)
            if(objectum.transform.position.y < waveHeight)
            {
                //Debug.Log("Applying forces");
                float displacementMultiplier = Mathf.Clamp01(-tform.position.y / depthBeforeSubmerged) * displacementAmount;
                floatingObjects[objectum].AddForce(new Vector3(0f, Mathf.Abs(Physics.gravity.y) * displacementMultiplier, 0f), ForceMode.Acceleration);
            }




        }
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject obj = other.gameObject;
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if(rb == null) { obj.GetComponentInChildren<Rigidbody>(); }
        if(rb == null) { return; }
        
        foreach(GameObject objectum in floatingObjects.Keys.ToList())
        {
            if(objectum == obj) { return; } //No duplicate objects allowed
        }
        //floatingObjects.Add(obj.transform.parent.gameObject);
        floatingObjects.Add(obj, rb);
        objectCount++;
    }

    private void OnTriggerExit(Collider other)
    {
        GameObject obj = other.gameObject;
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if(rb == null) { obj.GetComponentInChildren<Rigidbody>(); }
        if(rb == null) { return; }
        
        foreach(GameObject objectum in floatingObjects.Keys.ToList())
        {
            if(objectum == obj) { 
                floatingObjects.Remove(obj); 
                objectCount--;
            }
        }
    }








}
