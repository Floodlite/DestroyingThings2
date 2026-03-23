using UnityEngine;


public class CameraFollow : MonoBehaviour
{
    public GameObject player;

    void Start()
    {

    }

    void FixedUpdate()
    {
        transform.position = player.transform.position * 1.1f + new Vector3(20f, 40f, -3f);
    }

}
