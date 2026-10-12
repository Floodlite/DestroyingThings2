using UnityEngine;

[ExecuteAlways]
public class Fluids : MonoBehaviour
{
    [Header("Bilateral Blur Settings")]
    [Tooltip("The base radius for the blur in world space.")]
    public float worldRadius = 0.5f;

    [Tooltip("Caps how large the blur radius can get on screen to save performance.")]
    [Range(1, 64)]
    public int maxScreenSpaceRadius = 16;

    [Tooltip("How strongly the blur is applied")]
    [Range(0f, 1f)]
    public float strength = 1.0f;

    [Tooltip("Controls how strictly the blur respects depth edges, higher values prevent the liquid from bleeding into the background")]
    public float diffStrength = 5.0f; //Most important variable

    [Tooltip("Channels to apply the smoothing to RGB")]
    public Vector3 smoothMask = Vector3.one;

    public RenderTexture fluidDepthTexture;

    void Update()
    {
        if (fluidDepthTexture != null)
        {
            Shader.SetGlobalTexture("_BlitTexture", fluidDepthTexture);
        }

        //Pushes these variables globally so the URP Full Screen Pass can read them
        Shader.SetGlobalFloat("worldRadius", worldRadius);
        Shader.SetGlobalInt("maxScreenSpaceRadius", maxScreenSpaceRadius);
        Shader.SetGlobalFloat("strength", strength);
        Shader.SetGlobalFloat("diffStrength", diffStrength);
        Shader.SetGlobalVector("smoothMask", smoothMask);
    }
}
