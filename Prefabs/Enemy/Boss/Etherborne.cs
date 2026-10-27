using UnityEngine;
using UnityEngine.AI;
using Unity.Mathematics;
using UnityEngine.Splines;
using System;
using System.Collections;
using System.Collections.Generic;

public class Etherborne : MonoBehaviour
{
    private enum BossAttacks {
        idle=0, //Done
        wait=1, //Done
        warp=2, //Done
        changeMovement=3, //Done
        cableSlash=4, //Done
        callBackup=5, //Done
        coreShock=6, //Done
        sparkSurge=7, //Done?
        foeLightning=8, //Pending
        //spinAttack=9,
    }

    [SerializeField] private GameObject spark;
    [SerializeField] private GameObject sphere;
    private Vector3 startScale;
    [SerializeField] private GameObject[] slash;
    [SerializeField] private WaveMaker summonsPool;
    private Player[] players;
    private int slashIndex = 0;
    private float distanceToPlayer;
    private List<GameObject> sparks = new List<GameObject>();



    [SerializeField] private List<BossAttacks> attackSequence = new List<BossAttacks>();
    [SerializeField] private BossAttacks currentAttack = BossAttacks.idle;
    private BossAttacks basicAttack = BossAttacks.cableSlash;
    private BossAttacks superAttack = BossAttacks.foeLightning;
    [SerializeField] private int attackSequenceIndex = 0;
    [SerializeField] private float attackDurationMultiplier = 1f;
    private int sequenceLength = 4;

    [SerializeField] private bool attackInProgress = false;
    [SerializeField] private float cooldownTime = 5f; //Time between attacks
    [SerializeField] private float timeElapsed = 0f;
    [SerializeField] private int cyclesElapsed = 0;
    private int attackOptionCount = 9;
    private int chaseModeCount = 3;

    private enum ChaseMode
    {
        idle=0,
        pursue=1,
        retreat=2,
    }
    [SerializeField] private ChaseMode chaseMode = ChaseMode.idle;
    [SerializeField] private GameObject[] warpPoints;

    [SerializeField] private List<Reanimator> enemiesAlive = new List<Reanimator>();
    [SerializeField] private ConstructorConjunction constructors;
    [SerializeField] private WaveMachine waveMachine;
    [SerializeField] private EnemyChase enemyChase;
    private float originalSpeed = 10f;

    private int coreDamage = 6;
    private int slashDamage = 5; //TODO: Figure out all these enemy damage numbers once and for all
    private int sparkDamage = 4;
    private int lightningDamage = 6;
    private EnemyHealth enemyHealth;

    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private Gradient lightningWarmupColor; //Transluscent blue
    [SerializeField] private Gradient lightningActiveColor; //Neon cyan
    [SerializeField] private GameObject lightningSpark;
    

    private void Awake()
    {
        constructors = GetComponent<ConstructorConjunction>();
        enemyChase = GetComponent<EnemyChase>();
        enemyHealth = GetComponent<EnemyHealth>();
        splineContainer = GetComponent<SplineContainer>();
        attackOptionCount = Enum.GetNames(typeof(BossAttacks)).Length;
        chaseModeCount = Enum.GetNames(typeof(ChaseMode)).Length;
        CreateNewAttackSequence(sequenceLength);
    }

    private void Start()
    {
        SetAttackStatus(false);
        startScale = sphere.transform.localScale;
        originalSpeed = enemyChase.GetSpeed();
    }

    private void FixedUpdate()
    {
        if(!attackInProgress) {
            timeElapsed += Time.deltaTime;
        }

        if(!attackInProgress && timeElapsed >= cooldownTime)
        {
            FindEnemiesAlive();

            SetAttackStatus(true);
            timeElapsed = 0f;
            //currentAttack = PickRandomAttack();
            currentAttack = attackSequence[attackSequenceIndex];

            attackSequenceIndex++;
            if(attackSequenceIndex >= attackSequence.Count)
            {
                attackSequenceIndex = 0;
                sequenceLength++;
                CreateNewAttackSequence(sequenceLength);
                cyclesElapsed++;
            }
    


            switch(currentAttack)
            {
                //TODO: Use dictionary for attack durations, maybe
                case BossAttacks.idle:
                    StartCoroutine(DoNothing(7f));
                    attackDurationMultiplier += 0.1f;
                    break;
                case BossAttacks.wait:
                    StartCoroutine(DoNothing(4f));
                    break;
                case BossAttacks.changeMovement:
                    PickRandomChaseMode();
                    break;
                case BossAttacks.cableSlash:
                    StartCoroutine(SlashWindup(1f, 2.5f, 15f, 5f*attackDurationMultiplier));
                    break;
                case BossAttacks.coreShock:
                    StartCoroutine(CoreShock(sphere.transform.localScale.x, Mathf.Clamp(5f-0.15f*attackDurationMultiplier, 0.5f, 10f), 3f, 3f));
                    break;
                case BossAttacks.sparkSurge:
                    StartCoroutine(SparksWindup(3f, 3+cyclesElapsed, 4f*attackDurationMultiplier));
                    break;
                case BossAttacks.foeLightning:
                    StartCoroutine(LightningWindup(3f, 4f*attackDurationMultiplier, 4f));
                    break;
                case BossAttacks.warp:
                    WarpElsewhere();
                    break;
                case BossAttacks.callBackup:
                    CallBackup(3+cyclesElapsed);
                    break;
            }
        }
    }

    ///<summary>
    ///Changes the boss' movement behavior
    ///</summary>
    private void PickRandomChaseMode()
    {
        if(chaseMode == ChaseMode.idle)
        {
            enemyChase.SetSpeed(originalSpeed); //Reset speed back to normal
        }

        enemyChase.ToggleRole();

        int randy = UnityEngine.Random.Range(0, chaseModeCount);
        chaseMode = (ChaseMode)randy;
        
        switch(chaseMode) {
            case ChaseMode.idle:
                enemyChase.SetSpeed(originalSpeed/15f);
                enemyChase.SetChaseMode(EnemyChase.ChaseMode.pursue);
                break;
            case ChaseMode.pursue:
                enemyChase.SetChaseMode(EnemyChase.ChaseMode.pursue);
                break;
            case ChaseMode.retreat:
                enemyChase.SetChaseMode(EnemyChase.ChaseMode.retreat);
                break;
        }

        StartCoroutine(DoNothing(Mathf.Clamp(2f-0.1f*attackDurationMultiplier, 0.15f, 7.5f)));
    }

    private void SetChaseMode(ChaseMode newChaseMode)
    {
        chaseMode = newChaseMode;

        if(chaseMode == ChaseMode.idle)
        {
            enemyChase.SetSpeed(originalSpeed); //Reset speed back to normal
        }
        
        switch(newChaseMode) {
            case ChaseMode.idle:
                enemyChase.SetSpeed(originalSpeed/15f);
                enemyChase.SetChaseMode(EnemyChase.ChaseMode.pursue);
                break;
            case ChaseMode.pursue:
                enemyChase.SetChaseMode(EnemyChase.ChaseMode.pursue);
                break;
            case ChaseMode.retreat:
                enemyChase.SetChaseMode(EnemyChase.ChaseMode.retreat);
                break;
        }

        if(enemyChase != null) { enemyChase.ToggleRole(); }
    }

    ///<summary>
    ///Reshuffles the boss' attack order
    ///</summary>
    private void CreateNewAttackSequence(int length)
    {
        attackSequence.Clear();
        attackSequence.Add(BossAttacks.idle);
        if(enemiesAlive.Count <= 6) { attackSequence.Add(BossAttacks.callBackup); }
        
        for(int i=0; i<length; i++)
        {
            int randy = UnityEngine.Random.Range(4, attackOptionCount-1);
            attackSequence.Add((BossAttacks)randy);
            attackSequence.Add(BossAttacks.wait);
            attackSequence.Add(basicAttack);
            if(i>0 && i%3==0) { attackSequence.Add(BossAttacks.changeMovement); }
        }
        attackSequence.Add(superAttack);
        attackSequence.Add(BossAttacks.warp);
    }

    ///<summary>
    ///Does nothings
    ///</summary>
    private IEnumerator DoNothing(float boredomDuration)
    {
        yield return new WaitForSeconds(boredomDuration);
        SetAttackStatus(false);
    }

    ///<summary>
    ///Arcs a damaging laser through each enemy
    ///</summary>
    private IEnumerator LightningWindup(float windupTime, float attackDuration, float waitTime)
    {
        SetChaseMode(ChaseMode.idle);
        yield return new WaitForSeconds(windupTime/4f);
        StartCoroutine(FoeLightning(windupTime, attackDuration, waitTime));
    }

    private IEnumerator FoeLightning(float armTime, float attackDuration, float waitTime)
    {
        yield return new WaitForSeconds(attackDuration);
        /*Draws a line connecting each enemy
        * Create a spline
        * Create a knot for each enemy alive via FindObjectsOfType<EnemyHealth>(FindObjectsSortMode.InstanceID)
        * Set the knots' positions to their corresponding enemy
        * Have damaging projectiles move at very high speeds along the spline
        */
        int knotCapacity = enemiesAlive.Count+2; //First and last will be the boss


        //Line renderer configs
        //May cause hefty performance drops
        LineRenderer lineRenderer = this.gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.6f;
        lineRenderer.endWidth = 0.6f;
        lineRenderer.colorGradient = lightningWarmupColor;
        lineRenderer.positionCount = knotCapacity;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); //Placeholder

        Spline spline = new Spline(knotCapacity, false);
        spline.Add(this.transform.position); //First knot
        lineRenderer.SetPosition(0, this.transform.position);

        int knotIndex = 1; //Should be 1
        foreach(Reanimator enemy in enemiesAlive)
        {
            if(enemy == null) { continue; }
            float3 position = enemy.transform.parent.gameObject.transform.position;
            spline.Add(position);

            lineRenderer.SetPosition(knotIndex, position);
            knotIndex++;
        }

        //Reset spline in spline container
        Spline currentSpline = splineContainer.Spline;
        if(currentSpline != null)
        {
            SplineUtility.RemoveSpline(splineContainer, currentSpline);
        }

        spline.Add(this.transform.position); //Last knot
        lineRenderer.SetPosition(knotIndex, this.transform.position);

        SplineUtility.AddSpline(splineContainer, spline);
        
        yield return new WaitForSeconds(armTime);
        StartCoroutine(ChainLightning(splineContainer, spline, knotCapacity, attackDuration, waitTime, lineRenderer));
    }

    private IEnumerator ChainLightning(SplineContainer splineContainer, Spline spline, int knotCount, float attackDuration, float waitTime, LineRenderer lineRenderer)
    {
        List<GameObject> electrons = new List<GameObject>();

        //Spawn several high-speed projectiles that move along the spline
        for(int i=0; i<knotCount; i++)
        {
            float3 knotPosition = spline[i].Position;
            GameObject ball = Pooler.SpawnObject(lightningSpark, knotPosition, Quaternion.identity, Pooler.PoolType.bullets);
            HurtBox hurtBox = ball.GetComponentInChildren<HurtBox>();
            hurtBox.SetDamage(lightningDamage);
            hurtBox.SetHealthScript(enemyHealth);

            MoveAlongSpline splineMove = ball.GetComponentInChildren<MoveAlongSpline>();
            splineMove.ChangeSpline(splineContainer);

            electrons.Add(ball);
            yield return new WaitForSeconds(0.01f);
        }

        lineRenderer.colorGradient = lightningActiveColor;
        lineRenderer.startWidth = 1f;
        lineRenderer.endWidth = 1f;


        yield return new WaitForSeconds(attackDuration);

        StartCoroutine(TerminateSparks(electrons, 0f));
        if(lineRenderer != null)
        {
            Destroy(lineRenderer);
        }
        splineContainer.Spline = null;

        yield return new WaitForSeconds(waitTime);
        SetAttackStatus(false);
    }


    ///<summary>
    ///Fires a large projectile at the player that increases in speed and size over time
    ///</summary>
    private IEnumerator SlashWindup(float windupTime, float waitTime, float projectileSpeed, float slashDuration)
    {
        yield return new WaitForSeconds(windupTime);
        StartSlash(waitTime, projectileSpeed, slashDuration);
    }

    private void StartSlash(float waitTime, float projectileSpeed, float slashDuration)
    {
        Player closestPlayer = FindClosestPlayer();
        if (closestPlayer == null)
        {
            SetAttackStatus(false);
            return;
        }

        Vector3 directionToPlayer = closestPlayer.transform.position - transform.position;
        GameObject ball = Pooler.SpawnObject(slash[slashIndex], transform.position + new Vector3(0f, 1f, 0f), Quaternion.identity, Pooler.PoolType.bullets);
        
        HurtBox hurtBox = ball.GetComponent<HurtBox>();
        if(hurtBox != null)
        {
            //hurtBox.SetConstructors(constructors);
            hurtBox.SetDamage(slashDamage);
            hurtBox.SetHealthScript(enemyHealth);
        }
        
        //Notice: If projectiles are traveling absurdly fast, then normalize the player direction before multiplying
        Rigidbody ballRb = ball.GetComponent<Rigidbody>();
        ballRb.linearVelocity = directionToPlayer * projectileSpeed;

        /*
        ProjectileExpiration projectileExpiration = ball.GetComponentInChildren<ProjectileExpiration>();
        projectileExpiration.SetProjectileLifespan(slashDuration+5);
        projectileExpiration.StartSelfDestruct();
        */
    
        slashIndex++;
        if(slashIndex >= slash.Length) { slashIndex = 0; }
        StartCoroutine(LaunchSlash(ball, ballRb, waitTime, slashDuration, projectileSpeed, directionToPlayer));
    }

    private IEnumerator LaunchSlash(GameObject projectile, Rigidbody rb, float waitTime, float slashDuration, float projectileSpeed, Vector3 directionToPlayer)
    {
        float scaleSpeedMultiplier = 1f;
        float timeElapsed = 0f;
        float timeInterval = slashDuration / 20f;
        while(timeElapsed <= slashDuration)
        {
            //Projectiles get larger and fly faster as they move
            rb.linearVelocity = directionToPlayer * projectileSpeed * scaleSpeedMultiplier;
            projectile.transform.localScale *= scaleSpeedMultiplier;
            yield return new WaitForSeconds(timeInterval);
            timeElapsed += timeInterval;
            scaleSpeedMultiplier += 0.15f;
        }

        Pooler.ReleaseObjectToPool(projectile);
        yield return new WaitForSeconds(waitTime);
        SetAttackStatus(false);
    }

    private Player FindClosestPlayer()
    {
        players = FindObjectsByType<Player>(FindObjectsSortMode.InstanceID);
        if (players == null || players.Length == 0)
        {
            return null;
        }

        float closestDistance = 99999f;
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

    ///<summary>
    ///Spawns a sphere, waits for it to grow to full size, activates its hurtboxes, and then despawns it
    ///</summary>
    private IEnumerator CoreShock(float targetSize, float armTime, float upTime, float waitTime)
    {
        GameObject spawnedSphere = Pooler.SpawnObject(sphere, transform.position, Quaternion.identity, Pooler.PoolType.bullets);
        spawnedSphere.transform.localScale = new Vector3(1f, 1f, 1f);
        spawnedSphere.transform.rotation = UnityEngine.Random.rotation;
        ActivateHurtCollision(spawnedSphere, false);

        Vector3 growSize = startScale * targetSize;
        float attackTimeElapsed = 0f;
        while (attackTimeElapsed < armTime) //Verify
        {
            attackTimeElapsed += Time.deltaTime;
            float time = Mathf.Clamp01(attackTimeElapsed / armTime);
            spawnedSphere.transform.localScale = Vector3.Lerp(startScale, growSize, time);
            yield return null;
        }

        yield return new WaitForSeconds(0.05f);
        ActivateHurtCollision(spawnedSphere, true);
        yield return new WaitForSeconds(upTime);
        Pooler.ReleaseObjectToPool(spawnedSphere);

        yield return new WaitForSeconds(waitTime);
        SetAttackStatus(false);
    }

    private void ActivateHurtCollision(GameObject sphere, bool status)
    {
        foreach (HurtBox hurtBox in sphere.GetComponentsInChildren<HurtBox>(true))
        {
            if(hurtBox != null)
            {
                hurtBox.SetDamage(coreDamage);
                hurtBox.SetHealthScript(enemyHealth);
            }
        }

        foreach (Collider collider in sphere.GetComponentsInChildren<Collider>(true))
        {
            if(collider.enabled != status) { collider.enabled = status; }
        }
    }

    private void SetAttackStatus(bool newStatus)
    {
        attackInProgress = newStatus;
    }

    ///<summary>
    ///Teleports to a random warp point object in the scene
    ///</summary>
    private void WarpElsewhere()
    {
        if(warpPoints.Length <= 0) { 
            SetAttackStatus(false);
            return; 
        }
        int randy = UnityEngine.Random.Range(0, warpPoints.Length);
        this.transform.parent.transform.position = warpPoints[randy].transform.position;
        if(enemyChase != null) { enemyChase.WarpToNavMesh(); }
        SetAttackStatus(false);
    }

    ///<summary>
    ///Summons enemies around the boss
    ///</summary>
    private void CallBackup(int spawnCount)
    {
        for(int i=0; i<spawnCount; i++)
        {
            int randy = UnityEngine.Random.Range(0, summonsPool.GetLength());
            GameObject chosenEnemy = summonsPool.GetPool()[randy];

            Vector3 spawnOffset;
            switch(i) {
                case 0:
                    spawnOffset = new Vector3(7f, 0f, 0f); //E
                    break;  
                case 1:
                    spawnOffset = new Vector3(-7f, 0f, 0f); //W
                    break;
                case 2:
                    spawnOffset = new Vector3(0f, 0f, 7f); //N
                    break;
                case 3:
                    spawnOffset = new Vector3(0f, 0f, -7f); //S
                    break;
                case 4:
                    spawnOffset = new Vector3(7f, 0f, 7f); //NE
                    break;
                case 5:
                    spawnOffset = new Vector3(-7f, 0f, 7f); //NW
                    break;
                case 6:
                    spawnOffset = new Vector3(7f, 0f, -7f); //SE
                    break;
                case 7:
                    spawnOffset = new Vector3(-7f, 0f, -7f); //SW
                    break;
                default:
                    spawnOffset = new Vector3(0f, 0f, 0f); //Center
                    break;
            }

            GameObject spawnedEnemy = Pooler.SpawnObject(chosenEnemy, this.transform.position + spawnOffset, Quaternion.identity, Pooler.PoolType.spawnedEnemies);
            if(spawnedEnemy == null) { continue; }
            NavMeshAgent agent = spawnedEnemy.GetComponentInChildren<NavMeshAgent>();
            if(agent != null) { agent.Warp(spawnedEnemy.transform.position); }
            Reanimator reanimator = spawnedEnemy.GetComponent<Reanimator>();
            reanimator.SetResStatus(true);
        }
        SetAttackStatus(false);
    }

    ///<summary>
    ///Creates several damaging projectiles that move towards random enemies, move with the enemy for a few seconds, then seek out a new enemy
    ///(See SparkMove.cs for more information)
    ///</summary>
    private IEnumerator SparksWindup(float windupTime, int sparkCount, float waitTime)
    {
        yield return new WaitForSeconds(windupTime);
        StartCoroutine(LaunchSparks(sparkCount, waitTime));
    }

    private IEnumerator LaunchSparks(int sparkCount, float waitTime)
    {
        for(int i=0; i<sparkCount; i++)
        {
            GameObject projectile = Pooler.SpawnObject(spark, this.transform.position, Quaternion.identity, Pooler.PoolType.bullets);
            HurtBox hurtBox = projectile.GetComponentInChildren<HurtBox>();
            hurtBox.SetDamage(sparkDamage);
            hurtBox.SetHealthScript(enemyHealth);
            sparks.Add(projectile);
        }
        yield return new WaitForSeconds(waitTime);
        SetAttackStatus(false);

        yield return new WaitForSeconds(waitTime*2f);
        StartCoroutine(TerminateSparks(sparks, 0.75f));
    }

    private IEnumerator TerminateSparks(List<GameObject> sparks, float timeInterval)
    {
        foreach(GameObject spark in sparks)
        {
            Pooler.ReleaseObjectToPool(spark, Pooler.PoolType.bullets);
            yield return new WaitForSeconds(timeInterval);
        }
    }

    private void FindEnemiesAlive()
    {
        enemiesAlive.Clear();
        Reanimator[] enemiesFound = FindObjectsByType<Reanimator>(FindObjectsSortMode.None);
        foreach(Reanimator reanimator in enemiesFound)
        {
            if(!reanimator.IsDead())
            {
                enemiesAlive.Add(reanimator);
            }
        }
    }
}
