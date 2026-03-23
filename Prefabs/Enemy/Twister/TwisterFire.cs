using UnityEngine;
using System.Collections;
using UnityEngine.Pool;

public class TwisterFire : MonoBehaviour
{
    [SerializeField] private float attackFreq = 5f;
    [SerializeField] private float projectileSpeed = 0.5f;
    [SerializeField] private EnemyConstructor enemy;
    [SerializeField] private Rigidbody projectile;
    [SerializeField] private Vector3 playerLocation;
    [SerializeField] private GameObject player;
    [SerializeField] private ObjectPool<GameObject> pool;
    [SerializeField] private Player playerScript;
    [SerializeField] private float minPlayerDistance = 10f;
    [SerializeField] private float maxPlayerDistance = 70f;
    [SerializeField] private float distanceToPlayer;

    


    void Start()
    {
        playerScript = player.GetComponent<Player>();
        StartCoroutine(StartAttacking(attackFreq, projectileSpeed));
    }


    IEnumerator StartAttacking(float attackFreq, float projectileSpeed)
    {
        while(true) {
            distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if(distanceToPlayer > minPlayerDistance && distanceToPlayer < maxPlayerDistance) {
                Shoot(projectileSpeed);
            }
            yield return new WaitForSeconds(attackFreq);
        }
    }

    public void Shoot(float projectileSpeed)
    {
        playerLocation = player.transform.position - transform.position + new Vector3(0f, 0.5f, 0f);
        //playerLocation += playerScript.getPlayerMoveDirection() * 2;
        Rigidbody ball;
        //ball = Instantiate(projectile, transform.position - new Vector3(0f, -0.5f, 0f), transform.rotation); 
        ball = Pooler.SpawnObject(projectile, transform.position + new Vector3(0f, 1f, 0f), Quaternion.identity, Pooler.PoolType.bullets);
        ball.linearVelocity = transform.TransformDirection(playerLocation * projectileSpeed);
        //Debug.Log("Pew");
    }
        
}
