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
    private MeterMode currentMeterMode = MeterMode.none;
    private SpawnWave waveSpawner;
    private int dangerIncrease = 2;

    private void Awake()
    {
        scoreHandlerScripts = FindObjectsByType<PlayerScoreHandler>(FindObjectsSortMode.InstanceID);
        waveSpawner = FindObjectsByType<SpawnWave>(FindObjectsSortMode.InstanceID)[0];
        multiplierMultiplier = 1f;
        timeElapsed = 0f;
        cycleDuration = 45f;
        countdownActive = false;
        ChangePointMultipliers(1f, 1f);
        UpdatePlayerMeterMultipliers();
    }

    private void Start()
    {
        StartingGun();
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
        countdownActive = true;
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
        UpdatePlayerMeterMultipliers();
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
