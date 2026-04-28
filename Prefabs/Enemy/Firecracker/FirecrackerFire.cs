using UnityEngine;
using System.Collections;
using UnityEngine.Pool;
using UnityEngine.Splines;

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
                    //Shoot(projectileSpeed);
                    ShootII(projectileSpeed);
                }
                else
                {
                    //Shoot(projectileSpeed * 0.9f);
                    ShootII(projectileSpeed * 1.2f);
                }
            }
            yield return new WaitForSeconds(attackFreq);
        }
    }

    /// <summary>
    /// Creates a Bezier Curve going from your current position to the target's position
    /// </summary>
    private BezierCurve ConstructCurve(Vector3 yourPosition, Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - yourPosition;
        float distance = direction.magnitude;
        //Vector3 midPoint = yourPosition + direction * 0.5f;
        float arcHeight = distance * 0.3f;

        Vector3 coord1 = yourPosition + direction * 0.25f + Vector3.up * arcHeight;
        Vector3 coord2 = yourPosition + direction * 0.75f + Vector3.up * arcHeight;

        return new BezierCurve(yourPosition, coord1, coord2, targetPosition);
    }

    IEnumerator ThrowProjectile(GameObject ball, BezierCurve curve, float airTime)
    {
        float timeElapsed = 0f;
        while(timeElapsed < 1f)
        {
            timeElapsed += Time.deltaTime / airTime;
            float timeVar = Mathf.Clamp01(timeElapsed);
            ball.transform.position = Mathf.Pow(1 - timeVar, 3) * curve.P0 +
                          3 * Mathf.Pow(1 - timeVar, 2) * timeVar * curve.P1 +
                          3 * (1 - timeVar) * Mathf.Pow(timeVar, 2) * curve.P2 +
                          Mathf.Pow(timeVar, 3) * curve.P3;
            yield return null;
        }
        ball.GetComponent<Rigidbody>().isKinematic = false;
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
        ballRb.linearVelocity = directionToPlayer * projectileSpeed * 1.2f;
        //Debug.Log("Bam");
    }

    public void ShootII(float airTime)
    {
        Player closestPlayer = FindClosestPlayer();
        if (closestPlayer == null)
        {
            return;
        }

        GameObject ball = Pooler.SpawnObject(projectile, transform.position + new Vector3(0f, 1f, 0f), Quaternion.identity, Pooler.PoolType.bullets);
        BezierCurve curve = ConstructCurve(transform.position + new Vector3(0f, 1f, 0f), closestPlayer.transform.position);
        Rigidbody ballRb = ball.GetComponent<Rigidbody>();
        ballRb.isKinematic = true;
        StartCoroutine(ThrowProjectile(ball, curve, airTime));
        //Debug.Log("Blam");
    }
}
