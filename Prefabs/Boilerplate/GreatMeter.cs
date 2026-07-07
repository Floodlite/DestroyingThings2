using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
public class GreatMeter : MonoBehaviour
{
    [SerializeField] private int maxCycles = 8;
    [SerializeField] private int currentCycle = 0;
    [SerializeField] private float overallTimeElapsed = 0f;
    [SerializeField] private float timeElapsed = 0f;
    [SerializeField] private float cycleDuration = 45f; //seconds
    [SerializeField] private float globalEnemyMultiplier = 1f;
    [SerializeField] private float globalCollectMultiplier = 1f;
    [SerializeField] private float globalPointsMultiplier = 1f;
    [SerializeField] private float multiplierMultiplier = 1f; //Multiplies all the other multipliers

    private bool countdownActive = false;
    [SerializeField] private string formattedTime = "0:00.00";
    [SerializeField] private PlayerScoreHandler[] scoreHandlerScripts;
    private enum MeterMode
    {
        none=0,
        run=1,
        fight=2,
        runAndFight=3,
        boss=4,
        done=5,
    };
    [SerializeField] private MeterMode currentMeterMode = MeterMode.none;
    private SpawnWave waveSpawner;
    private int dangerIncrease = 2;
    [SerializeField] private float provokePercentage = 0f; //Controls how likely enemies are likely to switch from BASE to CHASE mode

    [SerializeField] private Dictionary<GameObject, bool> orbs = new Dictionary<GameObject, bool>(); //True: active
    [SerializeField] private List<GameObject> allOrbs = new List<GameObject>();
    private int startReactivatedOrbs = 0;
    private int cycleOrbsToReactivate = 0;

    private void Awake()
    {
        scoreHandlerScripts = FindObjectsByType<PlayerScoreHandler>(FindObjectsSortMode.InstanceID);
        waveSpawner = FindObjectsByType<SpawnWave>(FindObjectsSortMode.InstanceID)[0];
        
        multiplierMultiplier = 1f;
        timeElapsed = 0f;
        startReactivatedOrbs = 0;
        cycleDuration = 45f;
        countdownActive = false;
        ChangePointMultipliers(1f, 1f);
        UpdatePlayerMeterMultipliers();
        allOrbs.AddRange(GameObject.FindGameObjectsWithTag("Collect"));
        allOrbs.AddRange(GameObject.FindGameObjectsWithTag("Big Collect"));
        orbs = SetOrbDict();
    }

    private void Start()
    {
        StartCoroutine(StartingGun());
    }

    private void Update()
    {
        formattedTime = FormatTime(timeElapsed);
        if(countdownActive)
        {
            overallTimeElapsed += Time.deltaTime; 
            timeElapsed += Time.deltaTime;
            if(timeElapsed >= cycleDuration)
            {
                AdvanceCycles();
                Debug.Log(formattedTime);
                timeElapsed = 0f;
            }
        }
    }

    private IEnumerator StartingGun()
    {
        countdownActive = false;
        Debug.Log("3");
        yield return new WaitForSeconds(1f);
        Debug.Log("2");
        yield return new WaitForSeconds(1f);
        Debug.Log("1");
        yield return new WaitForSeconds(1f);
        Debug.Log("Go!");
        AdvanceCycles();
        InitOrbs(20f);
        countdownActive = true;
    }

    private Dictionary<GameObject, bool> SetOrbDict()
    {
        Dictionary<GameObject, bool> points = new Dictionary<GameObject, bool>();
        foreach(GameObject orb in allOrbs) {
            points.Add(orb, false); //All orbs should start deactivated
            UpdateOrb(orb, false);
        }
        return points;
    }

    private void UpdateOrb(GameObject orb, bool active)
    {
        orbs[orb] = active;

        MeshRenderer meshRenderer = orb.GetComponent<MeshRenderer>();
        Collider collider = orb.GetComponent<Collider>();
        if(meshRenderer != null)
        {
            meshRenderer.enabled = orbs[orb];
        }
        if(collider != null)
        {
            collider.enabled = orbs[orb];
        }
    }

    private bool IsOrbEnabled(GameObject orb)
    {
        MeshRenderer meshRenderer = orb.GetComponent<MeshRenderer>();
        if(meshRenderer != null) { return meshRenderer.enabled; }
        return false;
    }

    private void ReactivateOrbs()
    {
        int changedOrbs = 0;
        while(changedOrbs < cycleOrbsToReactivate) //Maybe add a "-1" to the right side of the condition just in case of rounding problems
        {
            int chosenIndex =  UnityEngine.Random.Range(0, allOrbs.Count); //Max-exclusive
            GameObject chosenOrb = allOrbs[chosenIndex];
            if(IsOrbEnabled(chosenOrb))
            {
                continue;
            }
            
            UpdateOrb(chosenOrb, true);
            changedOrbs++;
        }
    }

    private void InitOrbs(float percentage)
    {
        int orbCount = allOrbs.Count;
        if(orbCount % percentage != 0) { /*do something*/ }

        startReactivatedOrbs = (int) (orbCount * (percentage/100));
        cycleOrbsToReactivate = (orbCount - startReactivatedOrbs) / maxCycles;
        /*
        100 orbs
        10 cycles
        20 orbs should be activated at the start
        8 orbs should be activated each cycle
        */

        int changedOrbs = 0;
        while(changedOrbs < startReactivatedOrbs)
        {
            int chosenIndex =  UnityEngine.Random.Range(0, allOrbs.Count-1);
            GameObject chosenOrb = allOrbs[chosenIndex];
            if(IsOrbEnabled(chosenOrb))
            {
                continue;
            }
            
            UpdateOrb(chosenOrb, true);
            changedOrbs++;
        }
    }

    private void UpdatePlayerMeterMultipliers()
    {
        foreach(PlayerScoreHandler scoreHandler in scoreHandlerScripts)
        {
            scoreHandler.SetMeterMultiplierCollect(globalCollectMultiplier);
            scoreHandler.SetMeterMultiplierKill(globalEnemyMultiplier);
        }
    }

    private void ChangePointMultipliers(float collectMultiplier, float killMultiplier)
    {
        globalCollectMultiplier = collectMultiplier * multiplierMultiplier;
        globalEnemyMultiplier = killMultiplier * multiplierMultiplier;
    }

    private void SetMultiplierMultiplier(float newMultiplier=1f)
    {
        multiplierMultiplier = newMultiplier;
    }

    private void AdvanceCycles()
    {
        currentCycle++;
        if(currentCycle > 1)
        {
            multiplierMultiplier += 0.25f;
            cycleDuration += 5f;

            if(waveSpawner.GetNumberOfEnemiesAlive() <= 4) { dangerIncrease += 5; }
            waveSpawner.RaiseMaxDanger(dangerIncrease);
        }

        if(currentCycle == maxCycles - 1)
        {
            currentMeterMode = MeterMode.runAndFight;
        }
        else if(currentCycle >= maxCycles)
        {
            currentMeterMode = MeterMode.boss;
            SetMultiplierMultiplier(0.01f);
            waveSpawner.RaiseMaxDanger(-6);
        }

        else {
            switch(currentMeterMode) //Switches to the NEXT mode
            {
                case MeterMode.none:
                    currentMeterMode = MeterMode.run;
                    ChangePointMultipliers(1f, 0);
                    break;
                case MeterMode.run:
                    currentMeterMode = MeterMode.fight;
                    ChangePointMultipliers(0f, 1f);
                    break; 
                case MeterMode.fight:
                    currentMeterMode = MeterMode.run;
                    ChangePointMultipliers(1f, 0f);
                    break;
                case MeterMode.runAndFight:
                    currentMeterMode = MeterMode.run;
                    ChangePointMultipliers(1f, 1f);
                    break;
                case MeterMode.boss:
                    currentMeterMode = MeterMode.done; 
                    break;
                default:
                    currentMeterMode = MeterMode.run;
                    ChangePointMultipliers(1f, 0);
                    break;
            }
        }

        waveSpawner.NextWave();
        ReactivateOrbs();
        UpdatePlayerMeterMultipliers();
        AlterEnemies();
        provokePercentage = Mathf.Clamp(provokePercentage+15f, 80f, provokePercentage+10f);
    }

    private void AlterEnemies()
    {
        EnemyChase[] enemies = FindObjectsByType<EnemyChase>(FindObjectsSortMode.None);
        foreach(EnemyChase enemy in enemies)
        {
            if(enemy.GetEnemyRole() == EnemyChase.EnemyRole.CHASE)
            {
                continue;
            }

            int randy = UnityEngine.Random.Range(1, 101);
            if(randy >= provokePercentage)
            {
                enemy.SwitchRole(EnemyChase.EnemyRole.CHASE);
            }
        }
    }

    private string FormatTime(float timeElapsed)
    {
        int minutes = (int) timeElapsed / 60;
        int seconds = (int) timeElapsed % 60;
        float milliseconds = MathF.Round(timeElapsed, 2);
        string truncatedMilliseconds = ("" + milliseconds).Substring(1); //Removes the "0" from "0.55"
        return minutes + ":" + seconds + truncatedMilliseconds;

        /*
        * 200.55 seconds:
        * 3 minutes
        * 20 seconds
        * 0.55 milliseconds 
        */
    }
}
