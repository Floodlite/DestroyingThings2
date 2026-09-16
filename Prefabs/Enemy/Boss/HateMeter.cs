using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class HateMeter : MonoBehaviour
{
    [SerializeField] private bool activated = false;
    [SerializeField] private float RAW_Timer = 300f; //seconds
    [SerializeField] private string formattedTime = "00:00.00";
    [SerializeField] private PlayerHealth[] players;


    private void Starts()
    {
        players = FindObjectsByType<PlayerHealth>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    }

    private void FixedUpdate()
    {
        RAW_Timer -= Time.deltaTime;
        formattedTime = FormatTime(RAW_Timer);
        if(RAW_Timer <= 0f) { EndOfDays(); }
    }


    public void AddTime(float toAdd)
    {
        RAW_Timer += toAdd;
    }

    private void EndOfDays()
    {
        foreach(PlayerHealth player in players)
        {
            player.PermaDeath();
        }
    }

    public bool IsActivated()
    {
        return activated;
    }

    public void ChangeActivated(bool newStatus)
    {
        activated = newStatus;
    }

    private string FormatTime(float timeElapsed)
    {
        int minutes = (int) timeElapsed / 60;
        int seconds = (int) timeElapsed % 60;
        float milliseconds = MathF.Round(timeElapsed, 2);
        string truncatedMilliseconds = ("" + milliseconds).Substring(1);
        return minutes + ":" + seconds + truncatedMilliseconds;
    }
}
