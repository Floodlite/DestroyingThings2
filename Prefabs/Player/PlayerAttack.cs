using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private GameObject hurtBox;
    [SerializeField] private MeshRenderer mr;
    [SerializeField] private BoxCollider bc;
    [SerializeField] private bool punchInProgress = false;
    private Player player;
    private float boxSize = 0.2f;
    private float minBoxSize = 0.2f;
    private int punchCount = 1;

    [SerializeField] private RestraintMeter restraintMeterScript;

    private void Awake()
    {
        player = GetComponent<Player>();
        restraintMeterScript = GetComponent<RestraintMeter>();
        boxCollider = grabBox.GetComponent<Collider>();
    }

    private void Start()
    {
        punchCount = 1;
        cooldownInProgress = false;
    }

    public void BringTheHurt()
    {
        punchInProgress = !punchInProgress;
        hurtBox.gameObject.SetActive(punchInProgress);
    }

    public void BringTheHurtII(float punchUptime, float punchDimensions)
    {
        /*
        * Full punch cycle:
        * Attack button pressed
        * Shrink punch box to 0.2 on all sides, 
        * enable mesh renderer, 
        * grow to full size, 
        * enable box collider,
        * wait 1 second,
        * shrink box back down and disable the enabled components
        */

        if(restraintMeterScript.GetTurboStatus()) {
            //Debug.Log("Turbo valid");
            int pulverizeCount = restraintMeterScript.GetCurrentRestraint() / restraintMeterScript.GetTurboPunchCost(); 

            if(pulverizeCount > 0 && restraintMeterScript.SpendRestraint(pulverizeCount * restraintMeterScript.GetTurboPunchCost()))
            {
                punchCount = pulverizeCount;
                punchDimensions *= 1 + 0.05f * punchCount;
                //punchUptime = Mathf.Clamp(1.05f*punchUptime/punchCount, 0.01f, 1f);
                //Debug.Log("New uptime: " + punchUptime);
            }
        }

        if(punchCount < 1) {
            punchCount = 1;
        }

        if (!punchInProgress) {
            StartCoroutine(PunchCycle(punchUptime, punchDimensions, punchCount));
        }
        punchCount = 1;
    }
    
    
    IEnumerator PunchCycle(float punchDuration, float punchSize, int punchCount)
    {
        punchInProgress = true;

        for(int i=0; i<punchCount; i++) {
            mr.enabled = false;
            bc.isTrigger = true;
            bc.enabled = false;
            boxSize = 0.2f;

            hurtBox.transform.localScale = new Vector3(boxSize, boxSize, boxSize);
            yield return new WaitForSeconds(0.1f);
            mr.enabled = true;

                while (boxSize < punchSize)
                {
                    boxSize += 0.1f;
                    hurtBox.transform.localScale = new Vector3(boxSize, boxSize, boxSize);
                    yield return new WaitForSeconds(0.005f / (punchCount*1.05f));
                }
            
        

            bc.enabled = true;
            yield return new WaitForSeconds(punchDuration);

            
                while (boxSize > minBoxSize)
                {
                    boxSize -= 0.1f;
                    hurtBox.transform.localScale = new Vector3(boxSize, boxSize, boxSize);
                    yield return new WaitForSeconds(0.01f / (punchCount*1.05f));
                }
            
            hurtBox.transform.localPosition = new Vector3(0f, 1.5f, 2f);

            
            mr.enabled = false;
            bc.enabled = false;
        }
        punchInProgress = false;
    }



    [SerializeField] private bool holdingEnemy = false;
    [SerializeField] private bool enemyConstrained = false;
    [SerializeField] private GameObject heldObject;
    [SerializeField] private EnemyChase enemyChase;
    [SerializeField] private EnemyHealth heldHealth;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private GameObject grabBox;
    [SerializeField] private MeshRenderer grabMr;
    [SerializeField] private BoxCollider grabBc;
    [SerializeField] private float throwForce = 6f;
    private Collider boxCollider;
    private Rigidbody foeRb;
    private Collider heldCollider;
    private const float maxFallTime = 8f;

    public void ThrowHands(float punchUptime, float punchDimensions)
    {
        if (!punchInProgress) {
            StartCoroutine(GrabCycle(punchUptime, punchDimensions));
        }
    }
    
    IEnumerator GrabCycle(float punchDuration, float punchSize)
    {
        //Grow
        punchInProgress = true;
        for(int i=0; i<punchCount; i++) {
            grabMr.enabled = false;
            grabBc.isTrigger = true;
            grabBc.enabled = false;
            boxSize = 0.2f;
            grabBox.transform.localScale = new Vector3(boxSize, boxSize, boxSize);
            yield return new WaitForSeconds(0.1f);
            grabMr.enabled = true;
                while (boxSize < punchSize)
                {
                    boxSize += 0.1f;
                    grabBox.transform.localScale = new Vector3(boxSize, boxSize, boxSize);
                    yield return new WaitForSeconds(0.005f / (punchCount*1.05f));
                }
            grabBc.enabled = true;

            //Grab
            if(!holdingEnemy) {
                bool boxCast = Physics.BoxCast(boxCollider.bounds.center, transform.localScale, 
                    Vector3.forward, out RaycastHit objectHit, transform.rotation);
                heldObject = objectHit.collider.gameObject; 
                heldCollider = heldObject.GetComponentInChildren<Collider>();
                foeRb = heldObject.GetComponent<Rigidbody>();
                if(foeRb==null) { foeRb = heldObject.GetComponentInParent<Rigidbody>(); }
                if(foeRb==null) { foeRb = heldObject.GetComponentInChildren<Rigidbody>(); }
                if(foeRb!=null)
                {
                    enemyChase = heldObject.GetComponentInChildren<EnemyChase>();
                    agent = heldObject.GetComponentInChildren<NavMeshAgent>();
                    heldHealth = heldObject.GetComponentInChildren<EnemyHealth>();
                    if(enemyChase!=null) { enemyChase.enabled = false; }
                    if(agent!=null) { agent.enabled = false; }
                    if(heldHealth!=null && heldObject.GetComponentInChildren<Invincibility>()==null) { heldHealth.SetInvincibility(true); }
                    enemyConstrained = true;

                }
            }

            //Throw
            else
            {
                enemyConstrained = false;
                foeRb.AddForce(player.transform.forward*throwForce, ForceMode.Impulse);
                //TODO: Make the enemy be considered a damaging projectile while airborne
                StartCoroutine(ResetComponents());
            }


            //Shrink
            yield return new WaitForSeconds(punchDuration);
                while (boxSize > minBoxSize)
                {
                    boxSize -= 0.1f;
                    grabBox.transform.localScale = new Vector3(boxSize, boxSize, boxSize);
                    yield return new WaitForSeconds(0.01f / (punchCount*1.05f));
                }
            grabBox.transform.localPosition = new Vector3(0f, 1.5f, 2f);
            grabMr.enabled = false;
            grabBc.enabled = false;
        }
        punchInProgress = false;
    }

    private IEnumerator ResetComponents()
    {
        float timeElapsed = 0f;
        while(!Grounded() || timeElapsed >= maxFallTime)
        {
            yield return new WaitForSeconds(0.1f);
            timeElapsed += 0.1f;
        }

        if(enemyChase!=null) { 
            enemyChase.enabled = true; 
        }
        if(agent!=null) { 
            agent.enabled = true; 
        }
        if(heldHealth!=null && heldObject.GetComponentInChildren<Invincibility>()==null) { 
            heldHealth.SetInvincibility(false); 
        }
        heldObject = null;
        foeRb = null;
        agent.Warp(transform.position);
    }

    public bool Grounded()
    {
        if(heldCollider == null) { return false; }

        bool boxHit = Physics.BoxCast(heldCollider.bounds.center, transform.localScale * 0.75f, Vector3.down, out RaycastHit objectHit, transform.rotation, 1.1f);
        if (boxHit)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void Update()
    {
        if(enemyConstrained)
        {
            //Holds the object above the player's head
            heldObject.transform.position = player.transform.position + Vector3.up;
        }
    }




    [SerializeField] private bool cooldownInProgress = false;
    [SerializeField] private GameObject slingBall;
    [SerializeField] private GameObject rocket;
    [SerializeField] private GameObject wall;
    [SerializeField] private GameObject bomb;
    [SerializeField] private GameObject superBall;
    [SerializeField] private GameObject paintBall;

    public void DoomSword()
    {
        Debug.Log("Schwing!");
        return;
    }

    public void DoomSling()
    {
        PlayerShoot(slingBall, 2.5f, 2f, 0.15f);
    }

    public void DoomRocket()
    {
        PlayerShoot(rocket, 1.5f, 5f, 3f);
    }

    public void DoomTrowel()
    {
        PlayerShoot(wall, 0.1f, 5f, 2f);
    }

    public void DoomBomb()
    {
        PlayerShoot(bomb, 0.1f, 6f, 2f);
    }

    public void DoomBall()
    {
        PlayerShoot(superBall, 2f, 2f, 1f);
    }

    public void DoomPaint()
    {
        PlayerShoot(paintBall, 2.25f, 1.5f, 0.75f);
    }
    
    public void PlayerShoot(GameObject projectile, float projectileSpeed, float projectileLifespan, float cooldownTime)
    {
        if(projectile == null)
        {
            return;
        }

        if(!cooldownInProgress) {
            StartCoroutine(StartCooldown(cooldownTime));
            cooldownInProgress = true;
            GameObject ball = Pooler.SpawnObject(projectile, transform.position + new Vector3(0f, 0.5f, 2.5f), transform.rotation, Pooler.PoolType.bullets);
            Rigidbody ballRb = ball.GetComponent<Rigidbody>();
            if(ballRb != null) { ballRb.linearVelocity = player.GetPlayerMoveDirection() * projectileSpeed; }
            StartCoroutine(SelfDestruct(ball, projectileLifespan));
        }
    }

    private IEnumerator SelfDestruct(GameObject obj, float projectileLifespan)
    {
        yield return new WaitForSeconds(projectileLifespan);
        Pooler.ReleaseObjectToPool(obj, Pooler.PoolType.bullets);
    }

    private IEnumerator StartCooldown(float cooldownTime)
    {
        cooldownInProgress = true;
        yield return new WaitForSeconds(cooldownTime);
        cooldownInProgress = false;
    }
}
