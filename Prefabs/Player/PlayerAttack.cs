using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

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
        playerColliders = GetComponentsInChildren<Collider>();
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
        
        if(holdingEnemy) //Cannot punch while holding an object
        {
            ThrowHands(punchUptime, punchDimensions);
            return;
        }


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
    [SerializeField] private GameObject heldObject = null;
    [SerializeField] private EnemyChase enemyChase;
    [SerializeField] private EnemyHealth heldHealth;
    [SerializeField] private Projectile projectile;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private GameObject grabBox;
    [SerializeField] private MeshRenderer grabMr;
    [SerializeField] private BoxCollider grabBc;
    [SerializeField] private ConstructorConjunction constructors;
    [SerializeField] private Transform orignialParent;
    [SerializeField] private float throwForce = 10f;
<<<<<<< Updated upstream
=======
    [SerializeField] private float maxFallTime = 8f;
>>>>>>> Stashed changes
    private Collider boxCollider;
    private Rigidbody foeRb;
    private Collider heldCollider;
    private Collider[] heldColliders;
    private Collider[] playerColliders;
<<<<<<< Updated upstream
    private readonly Dictionary<Renderer, Color> originalEmissionColors = new Dictionary<Renderer, Color>();
    private const float maxFallTime = 8f;
    private Color thrownColor = new Color(143/255f, 61f/255f, 20/255f);
=======
    private string[] bannedTags = {"Floor", "Fluid", "Player", "Chain", "Collect", "Big Collect"}; //Banned tags
    private readonly Dictionary<Renderer, Color> originalEmissionColors = new Dictionary<Renderer, Color>();
    private Color thrownColor = new Color(143/255f, 61/255f, 20/255f);
>>>>>>> Stashed changes

    public void ThrowHands(float punchUptime, float punchDimensions)
    {
        if (!punchInProgress) {
            StartCoroutine(GrabCycle(punchUptime, punchDimensions));
        }
    }
    
    //P.S. I forgot that yield break existed
    IEnumerator GrabCycle(float punchDuration, float punchSize)
    {
        //Grow
        punchInProgress = true;
<<<<<<< Updated upstream
        for(int i=0; i<punchCount; i++) {
=======
        for(int i=0; i<1; i++) {
>>>>>>> Stashed changes
            grabMr.enabled = false;
            grabBc.isTrigger = true;
            grabBc.enabled = false;
            boxSize = 0.2f;
            grabBox.transform.localScale = new Vector3(boxSize, boxSize, boxSize);
<<<<<<< Updated upstream
            yield return new WaitForSeconds(0.1f);
            grabMr.enabled = true;
                while (boxSize < punchSize)
                {
                    boxSize += 0.1f;
                    grabBox.transform.localScale = new Vector3(boxSize, boxSize, boxSize);
                    yield return new WaitForSeconds(0.005f / (punchCount*1.05f));
                }
=======
            yield return new WaitForSeconds(0.05f);
            grabMr.enabled = true;
            while (boxSize < punchSize)
            {
                boxSize += 0.1f;
                grabBox.transform.localScale = new Vector3(boxSize, boxSize, boxSize);
                yield return new WaitForSeconds(0.005f);
            }
>>>>>>> Stashed changes
            grabBc.enabled = true;

            //Grab
            if(!holdingEnemy) {
                Collider[] grabHits = Physics.OverlapBox(
                    boxCollider.bounds.center,
                    boxCollider.bounds.extents,
                    boxCollider.transform.rotation,
                    ~0,
                    QueryTriggerInteraction.Ignore);
                heldObject = null;
                Rigidbody grabbedRigidbody = null;
                foreach (Collider grabHit in grabHits)
                {
<<<<<<< Updated upstream
=======
                    if (grabHit.transform == transform || grabHit.transform.IsChildOf(transform))
                    {
                        continue;
                    }

>>>>>>> Stashed changes
                    if (grabHit.attachedRigidbody != null)
                    {
                        heldObject = grabHit.gameObject;
                        grabbedRigidbody = grabHit.attachedRigidbody;
                        break;
                    }
                }
<<<<<<< Updated upstream
                if(heldObject != null && (heldObject.CompareTag("Fluid") || heldObject.CompareTag("Player") || heldObject.CompareTag("Floor"))) { //Banned tags
                    heldObject = null; 
=======
                if(heldObject != null && FoundInBannedTags()) {
                    heldObject = null; 
                    grabbedRigidbody = null;
>>>>>>> Stashed changes
                    Debug.Log("No grabbable object found");
                }
                if(heldObject != null) {
                    heldCollider = heldObject.GetComponent<Collider>();
                    foeRb = grabbedRigidbody;
                    if(foeRb==null) { foeRb = heldObject.GetComponent<Rigidbody>(); }
                    if(foeRb==null) { foeRb = heldObject.GetComponent<Rigidbody>(); }
                    if(foeRb==null) { foeRb = heldObject.GetComponentInParent<Rigidbody>(); }
                    if(foeRb==null) { foeRb = heldObject.GetComponentInChildren<Rigidbody>(); }
                    if(foeRb!=null) //Object is confirmed to be grabbable
                    {
                        heldObject = foeRb.gameObject;
                        constructors = heldObject.GetComponentInChildren<ConstructorConjunction>();
                        if(!heldObject.CompareTag("Enemy") || (heldObject.CompareTag("Enemy") && (constructors!=null && constructors.CanBeGrabbed())))
                        {
                            if(heldCollider==null) { heldCollider = heldObject.GetComponentInChildren<Collider>(); }
                            heldColliders = heldObject.GetComponentsInChildren<Collider>(true);
                            SetHeldCollisionIgnored(true);
                            holdingEnemy = true;
                            enemyChase = heldObject.GetComponentInChildren<EnemyChase>();
                            agent = heldObject.GetComponentInChildren<NavMeshAgent>();
                            heldHealth = heldObject.GetComponentInChildren<EnemyHealth>();
                            if(enemyChase!=null) { enemyChase.enabled = false; }
                            if(agent!=null) { agent.enabled = false; }
                            if(heldHealth!=null && heldObject.GetComponentInChildren<Invincibility>()==null) { heldHealth.SetInvincibility(true); }
                            enemyConstrained = true;
                            orignialParent = heldObject.transform.parent;
                            heldObject.transform.parent = null;
<<<<<<< Updated upstream
                            projectile = heldObject.AddComponent<Projectile>();
                            projectile.SetDamage(player.GetDamage());
                            foeRb.isKinematic = true;
                            if(heldObject!=null) { PulseColorThrownOn(heldObject); }
=======
                            foeRb.isKinematic = true;
>>>>>>> Stashed changes
                        }
                    }
                    else { Debug.Log("No rigidbody found"); }
                }
            }

            //Throw
<<<<<<< Updated upstream
            else if(heldObject != null && foeRb!=null) {   
=======
            else if(holdingEnemy && heldObject != null && foeRb!=null) { 
                projectile = heldObject.AddComponent<Projectile>();
                projectile.SetDamage(player.GetDamage());
>>>>>>> Stashed changes
                if(orignialParent!=null) { heldObject.transform.parent = orignialParent; }
                enemyConstrained = false;
                foeRb.isKinematic = false;
                SetHeldCollisionIgnored(false);
<<<<<<< Updated upstream
                foeRb.linearVelocity = player.rb.linearVelocity + player.transform.forward * throwForce;
                foeRb.angularVelocity = Vector3.zero;
                PulseColorThrownOff(heldObject);
=======

                Vector3 throwVector = player.rb.linearVelocity + player.transform.forward * throwForce;
                if(IsPathBlocked(throwVector, 1f)) { throwVector *= 0.6f; }

                foeRb.linearVelocity = throwVector;
                foeRb.angularVelocity = Vector3.zero;
                if(heldObject!=null) { PulseColorThrownOn(heldObject); }
>>>>>>> Stashed changes

                StartCoroutine(ResetComponents());
                holdingEnemy = false;
            }


            //Shrink
            yield return new WaitForSeconds(punchDuration);
                while (boxSize > minBoxSize)
                {
                    boxSize -= 0.1f;
                    grabBox.transform.localScale = new Vector3(boxSize, boxSize, boxSize);
<<<<<<< Updated upstream
                    yield return new WaitForSeconds(0.01f / (punchCount*1.05f));
=======
                    yield return new WaitForSeconds(0.002f);
>>>>>>> Stashed changes
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
        while(!Grounded() && timeElapsed < maxFallTime)
        {
            yield return new WaitForSeconds(0.1f);
            timeElapsed += 0.1f;
        }
<<<<<<< Updated upstream
=======
        yield return new WaitForSeconds(0.65f);
>>>>>>> Stashed changes

        if(enemyChase!=null) { 
            enemyChase.enabled = true; 
        }
        if(agent!=null) {
            agent.enabled = true; 
            //agent.Warp(transform.position);
        }
        if(heldHealth!=null && heldObject.GetComponentInChildren<Invincibility>()==null) { 
            heldHealth.SetInvincibility(false); 
        }
        if(projectile!=null) {
            projectile.YouHaveBeenDestroyed();
        }

        if(heldObject!=null) { PulseColorThrownOff(heldObject); }
        heldObject = null;
        foeRb = null;
    }

    public bool Grounded()
    {
        if(heldCollider == null) { return false; }

<<<<<<< Updated upstream
        bool boxHit = Physics.BoxCast(heldCollider.bounds.center, transform.localScale * 0.75f, Vector3.down, out RaycastHit objectHit, transform.rotation, 1.1f);
        if (boxHit)
        {
            return true;
        }
        else
        {
            return false;
        }
=======
        Bounds heldBounds = heldCollider.bounds;
        RaycastHit[] groundHits = Physics.BoxCastAll(
            heldBounds.center,
            heldBounds.extents * 0.9f,
            Vector3.down,
            heldObject.transform.rotation,
            0.2f,
            ~0,
            QueryTriggerInteraction.Ignore);

        foreach (RaycastHit groundHit in groundHits)
        {
            Transform hitTransform = groundHit.collider.transform;
            if (hitTransform == transform || hitTransform.IsChildOf(transform)) { continue; }
            if (heldObject != null && (hitTransform == heldObject.transform || hitTransform.IsChildOf(heldObject.transform))) { continue; }
            return true;
        }

        return false;
>>>>>>> Stashed changes
    }

    private void Update()
    {
        if(enemyConstrained && heldObject != null)
        {
            //Holds the object above the player's head
            heldObject.transform.position = player.transform.position + Vector3.up +
                new Vector3(0f, player.gameObject.transform.localScale.y+player.gameObject.transform.localScale.y*2f, 0f);
        }
        if(heldObject == null) //Deal with traps exploding while held
        {
            holdingEnemy = false;
            enemyConstrained = false;
        }
    }

    private void PulseColorThrownOn(GameObject thrownObject)
    {
        originalEmissionColors.Clear();
        foreach (Renderer renderer in thrownObject.GetComponentsInChildren<Renderer>(true))
        {
            if (!renderer.material.HasProperty("_EmissionColor")) { continue; }
            originalEmissionColors[renderer] = renderer.material.GetColor("_EmissionColor");
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", thrownColor);
        }
    }

    private void PulseColorThrownOff(GameObject thrownObject)
    {
        foreach (KeyValuePair<Renderer, Color> entry in originalEmissionColors)
        {
            if (entry.Key == null) { continue; }
            entry.Key.material.SetColor("_EmissionColor", entry.Value);
        }
        originalEmissionColors.Clear();
    }

    private void SetHeldCollisionIgnored(bool ignored)
    {
        if (heldColliders == null || playerColliders == null) { return; }

        foreach (Collider heldCollider in heldColliders)
        {
            if (heldCollider == null) { continue; }
            foreach (Collider playerCollider in playerColliders)
            {
                if (playerCollider == null || heldCollider == playerCollider) { continue; }
                Physics.IgnoreCollision(heldCollider, playerCollider, ignored);
            }
        }
    }

    private bool IsPathBlocked(Vector3 direction, float checkDistance)
    {
        if (direction.sqrMagnitude < 0.001f || checkDistance <= 0) {
            return false;
        }
<<<<<<< Updated upstream
        
        direction = direction.normalized;
        
        //SphereCast to check if path is blocked
        if (Physics.SphereCast(transform.position, 0.4f, direction, out RaycastHit hit, checkDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            //Only block if collision is far enough ahead (not micro-collisions)
            return (hit.distance > 0.1f);
        }
        
=======
        direction = direction.normalized;
        
        if (Physics.SphereCast(transform.position, 0.4f, direction, out RaycastHit hit, checkDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            return hit.distance > 0.1f;
        }
        return false;
    }

    private bool FoundInBannedTags() {
        foreach(string tag in bannedTags) {
            if(heldObject.CompareTag(tag))
            {
                return true;
            }
        }
>>>>>>> Stashed changes
        return false;
    }




<<<<<<< Updated upstream
=======

>>>>>>> Stashed changes
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
