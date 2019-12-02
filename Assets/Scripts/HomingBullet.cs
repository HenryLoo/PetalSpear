using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingBullet : Bullet
{
    public GameController Game;
    private Ship target;
    private float bulletSpeed;
    private Vector3 thisToTarget;

    // Use this for initialization
    new void Start()
    {
        base.Start();
        target = Team == 0 ? Game.CurrentOpponent : Game.CurrentPlayer;
    }

    // Update is called once per frame
    new void Update()
    {
        if( bulletSpeed == 0 )
            bulletSpeed = rb.velocity.magnitude;

        base.Update();

        if( target )
            thisToTarget = Vector3.Normalize( target.transform.position - transform.position );
    }

    void FixedUpdate()
    {
        if( target )
            rb.AddForce( thisToTarget * bulletSpeed * 2 );
    }
}