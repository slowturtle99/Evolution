using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plankton : MonoBehaviour
{
    /// Components
    private Rigidbody rb;
    
    /// Physical States
    public float mass = 0.05f;

    public void Die()
    {
        Destroy(gameObject);
    }

}
