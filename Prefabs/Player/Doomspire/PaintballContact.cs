using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PaintballContact : MonoBehaviour
{
    [SerializeField] private HashSet<Transform> objectsAlreadyInteractedWith = new HashSet<Transform>();
    [SerializeField] private bool useSharedMaterial = false;

    private void Start()
    {
        objectsAlreadyInteractedWith.Add(this.transform);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(objectsAlreadyInteractedWith.Contains(other.transform))
        {
            return;
        }
        objectsAlreadyInteractedWith.Add(other.transform);

        List<MeshRenderer> renderers = AddMeshRenderers(other.transform);
        if(renderers != null)
        {
            foreach (MeshRenderer renderer in renderers)
            {
                //Notice: Color values go from 0-1 instead of 0-255
                float redChange = Random.Range(0f, 1f);
                float greenChange = Random.Range(0f, 1f);
                float blueChange = Random.Range(0f, 1f);

                Material material = useSharedMaterial ? renderer.sharedMaterial : renderer.material;
                Color materialColor = material.color;

                float newRed = Mathf.Clamp01(materialColor.r + redChange);
                float newGreen = Mathf.Clamp01(materialColor.g + greenChange);
                float newBlue = Mathf.Clamp01(materialColor.b + blueChange);
                material.color = new Color(newRed, newGreen, newBlue);
            }
        }
    }

    private List<MeshRenderer> AddMeshRenderers(Transform parent)
    {
        List<MeshRenderer> renderers = new List<MeshRenderer>();
        Transform[] allTransforms = parent.GetComponentsInChildren<Transform>(true); 

        foreach (Transform childTransform in allTransforms)
        {
            MeshRenderer renderer = childTransform.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderers.Add(renderer);
            }
        }
        return renderers;
    }
}
