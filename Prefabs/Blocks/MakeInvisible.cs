using UnityEngine;

public class MakeInvisible : MonoBehaviour
{
    private bool invisible = true;

    private void Awake()
    {
        if(invisible) { MakeSelfInvisible(); }
    }

    private void MakeSelfInvisible() {
        MeshRenderer meshRenderer = this.GetComponent<MeshRenderer>();
        if(meshRenderer != null) { meshRenderer.enabled = false; }
    }

}
