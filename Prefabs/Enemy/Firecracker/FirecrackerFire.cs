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
        float distance = Mathf.Abs(Vector3.Distance(yourPosition, targetPosition));
        //y = -(2/distance * x)**2 + 1
        float x1 = distance*0.25f;
        float x2 = distance*0.75f;

        float fx1 = -Mathf.Pow(2/distance * x1, 2);
        float fx2 = -Mathf.Pow(2/distance * x2, 2);

        Vector3 coord1 = new Vector3(x1, fx1, yourPosition.z); //Should the z-coord be x1?
        Vector3 coord2 = new Vector3(x2, fx2, targetPosition.z);

        return new BezierCurve(yourPosition, coord1, coord2, targetPosition);
    }

    IEnumerator ThrowProjectile(GameObject ball, BezierCurve curve, float airTime)
    {
        float timeElapsed = 0f;
        yield return new WaitForSeconds(0.01f);
        while(ball.transform.position.x != curve.P3.x 
            && ball.transform.position.y != curve.P3.y
            && ball.transform.position.z != curve.P3.z)
        {
            timeElapsed += Time.deltaTime / airTime;
            ball.transform.position = Mathf.Pow(1 - timeElapsed, 2) * curve.P0 +
                               2 * (1 - timeElapsed) * timeElapsed * curve.P1 +
                               Mathf.Pow(timeElapsed, 2) * curve.P2;
            yield return new WaitForSeconds(airTime / timeElapsed); //TODO!
        }
        yield break;
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
        BezierCurve curve = ConstructCurve(transform.position, closestPlayer.transform.position);
        StartCoroutine(ThrowProjectile(ball, curve, airTime));
        //Debug.Log("Blam");
    }
}
