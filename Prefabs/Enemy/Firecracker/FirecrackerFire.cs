using UnityEngine;
using System.Collections;
using UnityEngine.Pool;

public class FirecrackerFire : MonoBehaviour
{
    [SerializeField] private float attackFreq;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private EnemyConstructor enemy;
    [SerializeField] private GameObject projectile = null;
    [SerializeField] private Vector3 playerLocation;
    [SerializeField] private GameObject player;
    [SerializeField] private ObjectPool<GameObject> pool;
    [SerializeField] private Player playerScript;
    [SerializeField] private float minPlayerDistance;
    [SerializeField] private float maxPlayerDistance;
    [SerializeField] private float distanceToPlayer;
    [SerializeField] private Player[] players;
    private Coroutine attackRoutine;
    [SerializeField] private ConstructorConjunction constructors;

    private void Start()
    {
        attackFreq = constructors.GetAttackFreq();
        projectileSpeed = constructors.GetProjectileSpeed();
        projectile = constructors.GetProjectile();
        minPlayerDistance = constructors.GetMinDistance();
        maxPlayerDistance = constructors.GetMaxDistance();
        StartAttackRoutine();
    }

    private void Awake()
    {
        constructors = GetComponent<ConstructorConjunction>();
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
                if(closestPlayer.Grounded()) {
                    Shoot(projectileSpeed);
                }
                else
                {
                    Shoot(projectileSpeed* 1.25f);
                }
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

        Vector3 directionToPlayer = closestPlayer.transform.position - transform.position;
        GameObject ball = Pooler.SpawnObject(projectile, transform.position + new Vector3(0f, 1f, 0f), Quaternion.identity, Pooler.PoolType.bullets);
        Rigidbody ballRb = ball.GetComponent<Rigidbody>();
        ballRb.linearVelocity = directionToPlayer * projectileSpeed;
        //Debug.Log("Bam");
    }
}
