using UnityEngine;
using Unity.AI.Navigation;

public class UpdateNavMesh : MonoBehaviour {

    public NavMeshSurface surface;

    private void Awake()
    {
        RebuildNavMesh();
    }

    private void Start()
    {
        RebuildNavMesh();
    }

    public void RebuildNavMesh()
    {
        surface.BuildNavMesh();
    }
}
