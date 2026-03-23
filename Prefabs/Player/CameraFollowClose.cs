using UnityEngine;



public class CameraFollowClose : MonoBehaviour
{
    public GameObject player;
    [SerializeField]
    private Vector3 offset = new Vector3(0f, 12f, -23f);

    private void Start()
    {

    }

    private void LateUpdate()
    {
        transform.position = player.transform.position + offset;
    }

    private void rotateLeft(float direction)
    {
        //transform.Rotate(Vector3.up, direction * 5f * Time.fixedDeltaTime);
    }
}
