using UnityEngine;
using Unity.AI.Navigation;
using System.Collections;

public class UpdateNavMesh : MonoBehaviour {

    public NavMeshSurface[] surfaces;
<<<<<<< Updated upstream
    public NavMeshSurface surface;
=======
>>>>>>> Stashed changes
    [SerializeField] private bool on = true;
    [SerializeField] private bool asyncMode = true;

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
