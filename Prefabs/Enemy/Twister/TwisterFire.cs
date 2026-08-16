using UnityEngine;
using System.Collections;
using UnityEngine.Pool;

public class TwisterFire : MonoBehaviour
{
    [SerializeField] private float attackFreq = 5f;
    [SerializeField] private float projectileSpeed = 0.5f;
    [SerializeField] private EnemyConstructor enemy;
    [SerializeField] private GameObject projectile;
    [SerializeField] private Vector3 playerLocation;
    [SerializeField] private GameObject player;
    [SerializeField] private ObjectPool<GameObject> pool;
    [SerializeField] private Player playerScript;
    [SerializeField] private float minPlayerDistance = 10f;
    [SerializeField] private float maxPlayerDistance = 70f;
    [SerializeField] private float distanceToPlayer;
    [SerializeField] private float projectileLifespan;
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
        projectileLifespan = constructors.GetProjectileLifespan();
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
        
        HurtBox hurtBox = ball.GetComponent<HurtBox>();
        if(hurtBox != null)
        {
            hurtBox.SetConstructors(constructors);
            EnemyHealth enemyHealth = GetComponent<EnemyHealth>(); //Didn't feel like giving this its own dedicated variable
            hurtBox.SetHealthScript(enemyHealth);
        }
        
        Rigidbody ballRb = ball.GetComponent<Rigidbody>();
        ballRb.linearVelocity = directionToPlayer * projectileSpeed;

        ProjectileExpiration projectileExpiration = ball.GetComponentInChildren<ProjectileExpiration>();
        projectileExpiration.StartSelfDestruct();    
        //Debug.Log("Pew");
    }

    /*Expiration logic has been moved to the projectile object itself
    private IEnumerator SelfDestruct(GameObject obj, float projectileLifespan)
    {
        yield return new WaitForSeconds(projectileLifespan);
        Pooler.ReleaseObjectToPool(obj, Pooler.PoolType.bullets);
    }  
    */
}
