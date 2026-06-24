using UnityEngine;
using Unity.AI.Navigation;

public class UpdateNavMesh : MonoBehaviour {

    public NavMeshSurface surface;
    public bool on = true;

    private void Start()
    {
        if(on) { 
            RebuildNavMesh(); 
            Debug.Log("Rebuild NavMesh is ON");
        }
        else
        {
            Debug.Log("Rebuild NavMesh is OFF");
        }

    }

    public void RebuildNavMesh()
    {
        surface.BuildNavMesh();
    }
}
