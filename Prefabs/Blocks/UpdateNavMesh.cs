using UnityEngine;
using Unity.AI.Navigation;

public class UpdateNavMesh : MonoBehaviour {

    public NavMeshSurface surface;
    void Start()
    {
        surface.BuildNavMesh();
    }
}
