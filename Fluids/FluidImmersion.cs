using UnityEngine;
using UnityEngine.AI;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class FluidImmersion : MonoBehaviour
{
    /*
    * FluidType: Certain type of fluid
    * Float: How much the player is immersed in this fluid (percentage(?))
    * Bool: Whether or not this fluid's immersion effects have been activated
    */
    private class FluidStatusEntry
    {
        public float ImmersionAmount;
        public bool EffectActive;

        public FluidStatusEntry(float ImmersionAmount, bool EffectActive)
        {
            this.ImmersionAmount = ImmersionAmount;
            this.EffectActive = EffectActive;
        }
    }
    private Dictionary<FluidType, FluidStatusEntry> FluidStatus 
        = new Dictionary<FluidType, FluidStatusEntry>(); 


    private Collider[] colliders;
    private float surfaceArea = 1f;
    [SerializeField] private float percentageImmersed = 0f;
    [SerializeField] private float fluidEffectThreshold = 25f; //Activate fluid effects once you are this % immersed in a fluid
    [SerializeField] private List<GameObject> collidingObjects = new List<GameObject>();


    //Player script counterparts
    [SerializeField] private bool isPlayer = true; //True: You are a player; False: You are my enemy
    private Player player;
    private PlayerHealth playerHealth;

    //Enemy script counterparts
    private NavMeshAgent agent;
    private EnemyChase enemyChase;
    private EnemyHealth enemyHealth;


    private void Awake() 
    {
        colliders = GetComponents<Collider>();
        surfaceArea = CalculateSurfaceArea(colliders);

        Player player = GetComponent<Player>();
        if(player == null) //Assign enemy scripts
        {
            isPlayer = false;
            agent = GetComponentInChildren<NavMeshAgent>();
            enemyChase = GetComponentInChildren<EnemyChase>();
            enemyHealth = GetComponentInChildren<EnemyHealth>();
        }
        else
        {
            playerHealth = GetComponent<PlayerHealth>();
        }
    }


    private void Start()
    {
        foreach (FluidType fluidType in Enum.GetValues(typeof(FluidType))) //Populates the dictionary
        {
            FluidStatus.Add(fluidType, new FluidStatusEntry(0f, false));
        }
    }


    private void Update()
    {
        foreach(FluidType fluidType in FluidStatus.Keys.ToList())
        {
            if(CalculateImmersionPercentage(fluidType) >= fluidEffectThreshold)
            {
                ActivateFluidEffect(fluidType); //TODO: Find better status effect apply/revert system
            }
        }
    }


    private float CalculateImmersionPercentage(FluidType fluidType)
    {
        return 100f * FluidStatus[fluidType].ImmersionAmount / surfaceArea;
    }

    private void ActivateFluidEffect(FluidType fluidType)
    {
        switch(fluidType)
        {
            case FluidType.water:
                //TODO
                break;
            case FluidType.saltwater:
                //TODO
                break;
            case FluidType.lava:
                //TODO
                break;
            case FluidType.blood:
                //TODO
                break;
            case FluidType.ichor:
                //TODO
                break;
            case FluidType.aether:
                //TODO
                break;
            case FluidType.sludge:
                //TODO
                break;
            case FluidType.coldwater:
                //TODO
                break;
            default:
                break;
        }
    }


    private float CalculateSurfaceArea(Collider[] colliders)
    {
        float totalArea = 0f;
        foreach(Collider collider in colliders)
        {
            totalArea += collider.bounds.size.magnitude;
        }
        return totalArea;
    }
    private float CalculateSurfaceArea(Collider collider)
    {
        return collider.bounds.size.magnitude;
    }
    private float CalculateSurfaceArea(SphereCollider sphereCollider)
    {
        return 4f*(float)Math.PI*(sphereCollider.radius*sphereCollider.radius);
    }
    

    private void ChangeSoakAmount(FluidType fluidType, float soakAmount)
    {
        FluidStatus[fluidType].ImmersionAmount += soakAmount;
    }


    private void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag("Fluid")) { return; }
        Fluid fluid = other.GetComponent<Fluid>();
        if(fluid == null) { return; }
        FluidType fluidType = fluid.GetFluidType();

        ChangeSoakAmount(fluidType, CalculateSurfaceArea(other as SphereCollider));
    }

    private void OnTriggerExit(Collider other)
    {
        if(!other.CompareTag("Fluid")) { return; }
        Fluid fluid = other.GetComponent<Fluid>();
        if(fluid == null) { return; }
        FluidType fluidType = fluid.GetFluidType();

        ChangeSoakAmount(fluidType, -1f*CalculateSurfaceArea(other as SphereCollider));


    }



}
