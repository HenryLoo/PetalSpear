using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {
    public GameObject HitSpark;

    public int Team;
    public int Damage;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

    }

    private void OnTriggerEnter( Collider other )
    {
        if (other.gameObject.tag == "Wall")
        {
            ProcessHitSpark();
            return;
        }

        Ship otherShip = other.gameObject.GetComponent<Ship>();
        if (otherShip && otherShip.Team != Team)
        {
            otherShip.Health -= Damage;
            ProcessHitSpark();
            return;
        }
    }

    private void ProcessHitSpark()
    {
        Instantiate( HitSpark, transform.position, transform.rotation );
        Destroy( this.gameObject );
    }
}