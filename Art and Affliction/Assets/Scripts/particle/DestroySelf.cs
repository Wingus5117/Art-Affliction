using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroySelf : MonoBehaviour
{
    public float DestroyDelay;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject,DestroyDelay);
    }

   
}
