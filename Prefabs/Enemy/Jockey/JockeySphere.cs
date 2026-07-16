using UnityEngine;

public class JockeySphere : MonoBehaviour
{
    [SerializeField] private JockeyChase jockeyChase;
    
    private void Awake()
    {
        jockeyChase = GetComponentInParent<JockeyChase>();
    }

    //Put this sphere as a child of the main enemy object
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            jockeyChase.OppFound(true);
        }
    }
}
