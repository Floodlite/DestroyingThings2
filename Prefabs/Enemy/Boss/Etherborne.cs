using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Etherborne : MonoBehaviour
{
    private enum BossAttacks {
        idle=0,
        wait=1,
        warp=2,
        foeLightning=3,
        coreShock=4,
        cableSlash=5,
        callBackup=6,
    }

    [SerializeField] private GameObject spark;
    [SerializeField] private GameObject wire;
    [SerializeField] private GameObject sphere;
    [SerializeField] private GameObject slash;



    [SerializeField] private List<BossAttacks> attackSequence = new List<BossAttacks>();
    [SerializeField] private BossAttacks currentAttack = BossAttacks.idle;
    [SerializeField] private float attackDurationMultiplier = 1f;

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
    

    private void Awake()
    {
        attackOptionCount = Enum.GetNames(typeof(BossAttacks)).Length;
        chaseModeCount = Enum.GetNames(typeof(ChaseMode)).Length;
        CreateAttackPattern(4);
    }

    private void Start()
    {
        attackInProgress = false;
    }

    private void FixedUpdate()
    {
        if(!attackInProgress) {
            timeElapsed += Time.deltaTime;
        }
        else if(!attackInProgress && timeElapsed >= cooldownTime)
        {
            attackInProgress = true;
            timeElapsed = 0f;
            currentAttack = PickRandomAttack();
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

    private ChaseMode PickRandomChaseMode()
    {
        int randy = UnityEngine.Random.Range(0, chaseModeCount);
        return (ChaseMode)randy;
    }

    private BossAttacks PickRandomAttack()
    {
        int randy = UnityEngine.Random.Range(3, attackOptionCount); //Exclude idle, wait, and warp actions
        return (BossAttacks)randy;
    }

    private void CreateAttackPattern(int length)
    {
        attackSequence.Clear();
        attackSequence.Add(BossAttacks.idle);
        for(int i=1; i<length; i++)
        {
            int randy = UnityEngine.Random.Range(2, attackOptionCount);
            attackSequence.Add((BossAttacks)randy);
            attackSequence.Add(BossAttacks.wait);
        }
    }


    private IEnumerator DoNothing(float boredomDuration)
    {
        yield return new WaitForSeconds(boredomDuration);
        attackInProgress = false;
    }

    private IEnumerator FoeLightning(float attackDuration)
    {
        yield return new WaitForSeconds(attackDuration);
        attackInProgress = false;
    }


  
}
