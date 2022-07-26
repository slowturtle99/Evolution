using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishManager : MonoBehaviour {

    const int threadGroupSize = 1024;

    Fish[] Fishes;
    Plankton[] Planktons;

    public int numFish = 0;
    public int numPlankton = 0;


    void Start () {
        
    }

    void Update () {
        numFish = FindObjectsOfType<Fish>().Length;
        numPlankton = FindObjectsOfType<Plankton>().Length;
        
    }

}
