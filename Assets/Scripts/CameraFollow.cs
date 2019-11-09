using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {
    public Transform Target;
    public float Distance = 15;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Target)
            transform.position = new Vector3( Target.position.x, Target.position.y + Distance, Target.position.z );
    }
}