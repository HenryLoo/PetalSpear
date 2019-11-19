using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlastZone : MonoBehaviour
{
    public int Team;
    public int Damage;
    public const float DURATION = 0.3f;
    private float currentDuration;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        currentDuration += Time.deltaTime;
        if( currentDuration >= DURATION )
        {
            Destroy( this.gameObject );
        }
    }

    private void OnTriggerEnter( Collider other )
    {
        Ship otherShip = other.gameObject.GetComponent<Ship>();
        if( otherShip && otherShip.Team != Team && otherShip.InvincibilityTimer == 0 )
        {
            otherShip.Health -= Damage;
            Destroy( this.gameObject );
            return;
        }
    }
}