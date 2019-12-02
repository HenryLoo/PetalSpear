using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmitterBullet : Bullet
{
    public GameController Game;
    private float emitterSpeed;

    public Weapon Weapon;
    private Weapon thisWeapon;

    public bool IsTargetingEnemy;
    private Ship target;
    private Vector3 thisToTarget;

    public float RotationSpeed;

    new void Awake()
    {
        base.Awake();
        thisWeapon = Instantiate( Weapon );
        thisWeapon.transform.SetParent( transform );
    }

    // Use this for initialization
    new void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    new void Update()
    {
        if( emitterSpeed == 0 )
            emitterSpeed = rb.velocity.magnitude;

        base.Update();

        if( IsTargetingEnemy )
        {
            target = Team == 0 ? Game.CurrentOpponent : Game.CurrentPlayer;
            if( target )
                thisToTarget = Vector3.Normalize( target.transform.position - transform.position );
        }
        else
        {
            thisToTarget = Vector3.Normalize( transform.forward );
        }

        if( target || !IsTargetingEnemy )
        {
            thisWeapon.Fire( transform.position, thisToTarget, rb.velocity, Team );
        }
        thisWeapon.UpdateValues();

        transform.rotation *= Quaternion.Euler( 0, RotationSpeed * Time.deltaTime, 0 );
    }

    new void OnTriggerEnter( Collider other )
    {
        // Don't collide with ships.
        Ship otherShip = other.gameObject.GetComponent<Ship>();
        if( otherShip )
            return;

        base.OnTriggerEnter( other );
    }
}