using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
    public int Health = 100;

    public int Team;

    public float RotationSpeed = 180;
    public float ThrustSpeed = 10;

    public WeaponTypes WeaponTypes;
    private Weapon standardWeapon;

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

        Weapon standard = WeaponTypes.GetWeapon( WeaponTypes.Type.Standard );
        standardWeapon = ( Weapon ) Instantiate( standard, transform.position, transform.rotation );
    }

    // Update is called once per frame
    void Update()
    {
        FrontVector = transform.up;
        
        trail.enabled = ( currentVelocity != Vector3.zero );

        standardWeapon.UpdateValues();

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
        standardWeapon.Fire( transform.position, FrontVector, currentVelocity, Team );
    }
}
