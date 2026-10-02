using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Etherborne : MonoBehaviour
{
    private enum BossAttacks {
        idle=0,
        wait=1,
        warp=2, //Done
        changeMovement=3, //Done
        callBackup=4,
        foeLightning=5,
        coreShock=6, //Done
        cableSlash=7, //Done
    }

    [SerializeField] private GameObject spark;
    [SerializeField] private GameObject wire;
    [SerializeField] private GameObject sphere;
    private Vector3 startScale;
    [SerializeField] private GameObject[] slash;
    private Player[] players;
    private int slashIndex = 0;
    private float distanceToPlayer;



    [SerializeField] private List<BossAttacks> attackSequence = new List<BossAttacks>();
    [SerializeField] private BossAttacks currentAttack = BossAttacks.idle;
    [SerializeField] private int attackSequenceIndex = 0;
    [SerializeField] private float attackDurationMultiplier = 1f;
    private int sequenceLength = 4;

    [SerializeField] private bool attackInProgress = false;
    [SerializeField] private float cooldownTime = 5f; //Time between attacks
    [SerializeField] private float timeElapsed = 0f;
    private int attackOptionCount = 7;
    private int chaseModeCount = 3;

    private enum ChaseMode
    {
        idle=0,
        chase=1,
        retreat=2,
    }
    [SerializeField] private ChaseMode chaseMode = ChaseMode.idle;
    [SerializeField] private GameObject[] warpPoints;

    [SerializeField] List<EnemyHealth> enemiesAlive = new List<EnemyHealth>();
    [SerializeField] private ConstructorConjunction constructors;
    [SerializeField] private EnemyChase enemyChase;
    [SerializeField] private WaveMachine waveMachine;
    

    private void Awake()
    {
        constructors = GetComponent<ConstructorConjunction>();
        enemyChase = GetComponent<EnemyChase>();
        attackOptionCount = Enum.GetNames(typeof(BossAttacks)).Length;
        chaseModeCount = Enum.GetNames(typeof(ChaseMode)).Length;
        CreateNewAttackSequence(sequenceLength);
    }

    private void Start()
    {
        SetAttackStatus(false);
        startScale = sphere.transform.localScale;
    }

    private void FixedUpdate()
    {
        if(!attackInProgress) {
            timeElapsed += Time.deltaTime;
        }
        else if(!attackInProgress && timeElapsed >= cooldownTime)
        {
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
            }
        }

        if(attackInProgress)
        {
            switch(currentAttack)
            {
                case BossAttacks.idle:
                    StartCoroutine(DoNothing(7f));
                    attackDurationMultiplier += 0.1f;
                    break;
                case BossAttacks.wait:
                    StartCoroutine(DoNothing(4f));
                    break;
                case BossAttacks.foeLightning:
                    //TODO: Use dictionary for attack duartions, maybe
                    StartCoroutine(FoeLightning(8f*attackDurationMultiplier));
                    break;
                
            }
        }
    }

    private void PickRandomChaseMode()
    {
        int randy = UnityEngine.Random.Range(0, chaseModeCount);
        chaseMode = (ChaseMode)randy;
        
        switch(chaseMode) {
            case ChaseMode.idle:
                //TODO: Manipulate EnemyChase agent status
                break;
        }

        if(enemyChase != null) { enemyChase.ToggleRole(); }
    }

    private void CreateNewAttackSequence(int length)
    {
        attackSequence.Clear();
        attackSequence.Add(BossAttacks.idle);
        if(enemiesAlive.Count <= 5) { attackSequence.Add(BossAttacks.callBackup); }
        
        for(int i=0; i<length; i++)
        {
            int randy = UnityEngine.Random.Range(5, attackOptionCount);
            attackSequence.Add((BossAttacks)randy);
            attackSequence.Add(BossAttacks.wait);
            if(i>0 && i%2==0) { attackSequence.Add(BossAttacks.changeMovement); }
        }
        attackSequence.Add(BossAttacks.warp);
    }


    private IEnumerator DoNothing(float boredomDuration)
    {
        yield return new WaitForSeconds(boredomDuration);
        SetAttackStatus(false);
    }

    private IEnumerator FoeLightning(float attackDuration)
    {
        yield return new WaitForSeconds(attackDuration);
        SetAttackStatus(false);
    }

    private void StartSlash(float waitTime, float projectileSpeed, float slashDuration)
    {
        Player closestPlayer = FindClosestPlayer();
        if (closestPlayer == null)
        {
            return;
        }

        Vector3 directionToPlayer = closestPlayer.transform.position - transform.position;
        GameObject ball = Pooler.SpawnObject(slash[slashIndex], transform.position + new Vector3(0f, 1f, 0f), Quaternion.identity, Pooler.PoolType.bullets);
        
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
            rb.transform.parent.gameObject.transform.localScale *= scaleSpeedMultiplier;
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

    private IEnumerator CoreShock(float targetSize, float armTime, float upTime, float waitTime)
    {
        GameObject spawnedSphere = Pooler.SpawnObject(sphere, transform.position, Quaternion.identity, Pooler.PoolType.bullets);
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
        foreach (Collider collider in sphere.GetComponentsInChildren<Collider>(true))
        {
            if(!collider.enabled) { collider.enabled = status; }
        }
    }

    private void SetAttackStatus(bool newStatus)
    {
        attackInProgress = newStatus;
    }

    private void WarpElsewhere()
    {
        int randy = UnityEngine.Random.Range(0, warpPoints.Length);
        this.transform.parent.transform.position = warpPoints[randy].transform.position;
        if(enemyChase != null) { enemyChase.WarpToNavMesh(); }
    }
  
}
