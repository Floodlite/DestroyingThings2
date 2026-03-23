using UnityEngine;

public class Reanimator : MonoBehaviour
{
    private GameObject self;

    private void start()
    {
        self = this.transform.GetChild(0).gameObject;
    }
    public void reEnable()
    {
        self.SetActive(true);
    }
}
