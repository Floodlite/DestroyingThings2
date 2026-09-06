using UnityEngine;

public class WaterWave : MonoBehaviour
{
    public static WaterWave instance; //Singleton behavior

    [SerializeField] private float amplitude = 1f;
    [SerializeField] private float length = 2f;
    [SerializeField] private float speed = 1f;
    [SerializeField] private float offset = 0f;


    private void Awake()
    {
        if(instance = null) { instance = this; }
        else if(instance != this)
        {
            Debug.Log("Instance already exists... destroying object!");
            Destroy(this);
        }
    }

    private void Update()
    {
        offset += Time.deltaTime * speed;
    }

    public float GetWaveHeight(float x)
    {
        return amplitude * Mathf.Sin(x / length);
    }
    //TODO: Move all of this to Buoyancy
}
