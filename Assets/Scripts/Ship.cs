using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
    public int Health = 100;

    public int Team;

    public float RotationSpeed = 180;
    public float ThrustSpeed = 10;

    public Rigidbody Bullet;
    public float BulletSpeed = 20;
    public int BulletDamage = 10;
    public float BulletsPerSecond = 4;
    private float currentFireDelay;

    private Rigidbody rb;
    private Vector3 currentVelocity;

    public Vector3 FrontVector;

    public GameObject Explosion;

    private TrailRenderer trail;

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        trail = GetComponent<TrailRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        FrontVector = transform.up;
        
        trail.enabled = ( currentVelocity != Vector3.zero );

        if (currentFireDelay > 0)
        {
            currentFireDelay -= Time.deltaTime;
            currentFireDelay = Mathf.Max( currentFireDelay, 0 );
        }

        if (Health <= 0)
        {
            GameObject effect = Instantiate( Explosion, transform.position, transform.rotation );
            Destroy( this.gameObject );
        }
    }

    void FixedUpdate()
    {
        rb.AddForce( currentVelocity );
    }

    void OnCollisionEnter( Collision other )
    {
        if( other.gameObject.tag == "Character" )
        {
            Physics.IgnoreCollision( other.gameObject.GetComponent<Collider>(), GetComponent<Collider>() );
        }
    }


    public void Thrust(float amount)
    {
        if( amount != 0 )
        {
            float thrust = Mathf.Sign( amount );
            currentVelocity = FrontVector * thrust * ThrustSpeed;
        }
        else
        {
            currentVelocity = new Vector3( 0, 0, 0 );
        }

    }

    public void Rotate(float amount)
    {
        if( amount != 0 )
        {
            float rotation = -Mathf.Sign( amount );
            transform.rotation *= Quaternion.Euler( 0, 0, rotation * RotationSpeed * Time.deltaTime );
        }
    }

    public void Fire()
    {
        if (currentFireDelay == 0)
        {
            Rigidbody bulletInstance = ( Rigidbody ) Instantiate( Bullet, transform.position, transform.rotation );
            bulletInstance.velocity = FrontVector * ( BulletSpeed + Vector3.Magnitude( currentVelocity ) );
            Bullet bullet = bulletInstance.GetComponent<Bullet>();
            bullet.Team = Team;
            bullet.Damage = BulletDamage;
            currentFireDelay = 1 / BulletsPerSecond;
        }
    }
}
