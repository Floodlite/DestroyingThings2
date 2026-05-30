using UnityEngine;

public class RestraintMeter : MonoBehaviour
{
   [SerializeField] private int max = 16;
   [SerializeField] private int current = 0;

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
    }

    public void MultiplyRestraint(float multiplier)
    {
        current = (int) (current * multiplier);
        if(current >= max)
        {
            current = max;
        }
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
}
