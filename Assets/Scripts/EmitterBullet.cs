using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmitterBullet : Bullet
{
    public GameController Game;
    private Ship target;
    private float emitterSpeed;
    private Rigidbody rb;
    private Vector3 thisToTarget;

    public Weapon Weapon;
    private Weapon thisWeapon;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        thisWeapon = Instantiate( Weapon );
        thisWeapon.transform.SetParent( transform );
    }

    // Use this for initialization
    void Start()
    {
        target = Team == 0 ? Game.CurrentOpponent : Game.CurrentPlayer;
    }

    // Update is called once per frame
    new void Update()
    {
        if( emitterSpeed == 0 )
            emitterSpeed = rb.velocity.magnitude;

        base.Update();

        if( target )
        {
            thisToTarget = Vector3.Normalize( target.transform.position - transform.position );
            thisWeapon.Fire( transform.position, thisToTarget, rb.velocity, Team );
        }
        thisWeapon.UpdateValues();
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