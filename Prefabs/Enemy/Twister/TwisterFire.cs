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
    [SerializeField] private Player[] players;
    private Coroutine attackRoutine;


    private void Awake()
    {
        EnsurePlayerReference();
    }

    private void OnEnable()
    {
        StartAttackRoutine();
    }

    private void OnDisable()
    {
        StopAttackRoutine();
    }

    private void EnsurePlayerReference()
    {
        if (player != null && playerScript == null)
        {
            playerScript = player.GetComponent<Player>();
        }
    }

    private Player FindClosestPlayer()
    {
        players = FindObjectsByType<Player>(FindObjectsSortMode.InstanceID);
        if (players == null || players.Length == 0)
        {
            return null;
        }

        float distanceToPlayer = 0;
        float closestDistance = 99999;
        int indexOfClosest = 0;

        for(int i=0; i<players.Length; i++)
        {
            distanceToPlayer = Vector3.Distance(transform.position, players[i].transform.position);
            
            if(distanceToPlayer < closestDistance)
            {
                closestDistance = distanceToPlayer;
                indexOfClosest = i;
            }
        }
        return players[indexOfClosest];
    }

    private void StartAttackRoutine()
    {
        StopAttackRoutine();
        attackRoutine = StartCoroutine(StartAttacking(attackFreq, projectileSpeed));
    }

    private void StopAttackRoutine()
    {
        if (attackRoutine == null)
        {
            return;
        }

        StopCoroutine(attackRoutine);
        attackRoutine = null;
    }

    IEnumerator StartAttacking(float attackFreq, float projectileSpeed)
    {
        while(true) {
            Player closestPlayer = FindClosestPlayer();
            if (closestPlayer == null)
            {
                yield return new WaitForSeconds(attackFreq);
                continue;
            }

            player = closestPlayer.gameObject;
            distanceToPlayer = Vector3.Distance(transform.position, closestPlayer.transform.position);
            if(distanceToPlayer > minPlayerDistance && distanceToPlayer < maxPlayerDistance) {
                Shoot(projectileSpeed);
            }
            yield return new WaitForSeconds(attackFreq);
        }
    }

    public void Shoot(float projectileSpeed)
    {
        Player closestPlayer = FindClosestPlayer();
        if (closestPlayer == null)
        {
            return;
        }

        playerLocation = closestPlayer.transform.position - transform.position + new Vector3(0f, 0.5f, 0f);
        Rigidbody ball;
        ball = Pooler.SpawnObject(projectile, transform.position + new Vector3(0f, 1f, 0f), Quaternion.identity, Pooler.PoolType.bullets);
        ball.linearVelocity = transform.TransformDirection(playerLocation * projectileSpeed);
        //Debug.Log("Pew");
    }
        
}
