using UnityEngine;

public class RestraintMeter : MonoBehaviour
{
   [SerializeField] private int max = 16;
   [SerializeField] private int current = 0;
   private int timesActivated = 0;
   private int startingRestraint = 4;

   [SerializeField] private bool turboMode = false;
   [SerializeField] private int turboJumpCost = 4;
   [SerializeField] private int bonusJumpCost = 3;
   [SerializeField] private int turboPunchCost = 4; //Adds extra punches
   [SerializeField] private int turboHoofCost = 3;

   private bool debugMode = false;

   private void Start()
    {
        if(max < 2)
        {
            max = 16;
        }
        current = startingRestraint;

        if(debugMode) { current = 48; }
    }

    public bool SpendRestraint(int cost)
    {
        if(cost > current)
        {
            return false;
        }
        current -= cost;
        return true;
    }

    public void AddRestraint(int amount)
    {
        current += amount;
        FullMeterTrigger();
    }

    //Does not let the amount added fill the meter to its max
    public void AddRestraintClamped(int amount)
    {
        current += amount;
        if(current >= max)
        {
            current = max - 2;
        }
    }

    public void MultiplyRestraint(float multiplier)
    {
        current = (int) (current * multiplier);
        FullMeterTrigger();
    }

    public void ChangeMaxRestraint(float multiplier)
    {
        max = (int) (max * multiplier);
        
        if(max < 2)
        {
            max = 2;
        }

        if(current >= max)
        {
            current = max - 1;
        }
    }

    public void FullMeterTrigger()
    {
        if(current >= max)
        {
            PlayerHealth playerHealthScript = GetComponent<PlayerHealth>();
            if(playerHealthScript != null)
            {
                playerHealthScript.GainHP(4);
            }

            if(current > max)
            {
                current = max;
            }

            timesActivated++;
            float lossMultiplier = Mathf.Clamp(0.9f-0.2f*timesActivated, 0.1f, 0.9f-0.2f*timesActivated);
            MultiplyRestraint(lossMultiplier);

            //TODO: Stat boosts
        }
    }

    public bool GetTurboStatus()
    {
        return turboMode;
    }

    public int GetMaxRestraint()
    {
        return max;
    }

    public int GetCurrentRestraint()
    {
        return current;
    }

    public void ToggleTurbo()
    {
        turboMode = !turboMode;
    }

    public int GetTurboJumpCost()
    {
        return turboJumpCost;
    }

    public int GetBonusJumpCost()
    {
        return bonusJumpCost;
    }

    public int GetTurboHoofCost()
    {
        return turboHoofCost;
    }

    public int GetTurboPunchCost()
    {
        return turboPunchCost;
    }
}
