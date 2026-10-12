using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;


//Global
public enum FluidType
{
    water,
    saltwater,
    lava,
    blood,
    ichor,
    aether,
    sludge,
    coldwater,
}


public class Fluid : MonoBehaviour
{
    [SerializeField] private FluidType fluidType = FluidType.water;

    public FluidType GetFluidType()
    {
        return fluidType;
    }
}
