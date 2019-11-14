using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextDestroy : MonoBehaviour
{
    public float Duration;
    private float currentDuration;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        currentDuration += Time.deltaTime;
        if( currentDuration >= Duration )
        {
            Destroy( this.gameObject );
        }
    }
}
