using UnityEngine;
using Unity.AI.Navigation;

public class UpdateNavMesh : MonoBehaviour {

    public NavMeshSurface[] surfaces;
    public bool on = true;

    private void Awake()
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
        foreach(NavMeshSurface surface in surfaces) {
            surface.BuildNavMesh();
        }
    }
}
