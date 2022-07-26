using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
public class FishSetting : ScriptableObject
{
    [Header ("General")]
    public Fish FishPrefab;
    public LayerMask FishMask;
    public LayerMask PlanktonMask;
    public LayerMask obstacleMask;

    public float maxViewingRange = 1.0f;

    [Header ("Physical")]
    public float fishPrefabDefaultVolume = 4.0f/3.0f*Mathf.PI*0.2f*0.5f*1.0f;

    [Header ("Physiological")]
    public float mutationRate = 0.05f;
    public float geneDiffLimit = 0.1f;
    public float maxAge = 60.0f;
    public float childAdultRatio = 0.2f;
    public float predationMassRatio = 0.5f;

    [Header ("Control")]
    public float alignWeight = 1.0f;
    public float cohesionWeight = 1.0f;
    public float seperationWeight = 1.0f;
    public float mateFollowWeight = 1.0f;
    public float preyChaseWeight = 1.0f;
    public float predatorAvoidWeight = 1.0f;
    public float obstacleAvoidWeight = 2.0f;


    public float maxSpeed = 5;
    public float maxSteerForce = 3;

    [Header ("Energy & Force")]
    public float dragCoeff = 0.04f;
    public float basalMetabolismCoeff = 0.0f;
    public float predationEfficiency = 1.0f;
    public float birthEfficiency = 0.5f;
    
    public float maxSpeedCoeff = 1.0f;
    public float idleSpeedCoeff = 1.0f;
    
    
}
