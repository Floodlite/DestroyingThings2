using UnityEngine;

public class RestraintMeter : MonoBehaviour
{
   [SerializeField] private int max = 16;
   [SerializeField] private int current = 0;
   [SerializeField] private int timesActivated = 0;

   private void Start()
    {
        if(max < 2)
        {
            max = 16;
        }
        current = 0;
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

    //Does not let the amount added fill the meter
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
}
