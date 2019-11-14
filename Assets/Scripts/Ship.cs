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
    private Weapon heavyWeapon;

    private Rigidbody rb;
    private Vector3 currentVelocity;

    public Vector3 FrontVector;

    public GameObject Explosion;

    private TrailRenderer trail;

    private Renderer rend;
    private Color colourStart = Color.white;
    private Color colourEnd = Color.white;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        trail = GetComponent<TrailRenderer>();
        rend = GetComponent<Renderer>();
    }

    // Use this for initialization
    void Start()
    {
        Weapon standard = WeaponTypes.GetWeapon( WeaponTypes.Type.Standard );
        standardWeapon = ( Weapon ) Instantiate( standard, transform.position, transform.rotation );
        standardWeapon.transform.SetParent( transform );
    }

    // Update is called once per frame
    void Update()
    {
        FrontVector = transform.up;

        trail.enabled = ( currentVelocity != Vector3.zero );

        standardWeapon.UpdateValues();

        if( heavyWeapon )
        {
            heavyWeapon.UpdateValues();
            colourEnd = heavyWeapon.Colour;
        }
        else
        {
            colourEnd = Color.white;
        }

        float lerp = Mathf.PingPong( Time.time, 1 ) / 1;
        rend.material.color = Color.Lerp( colourStart, colourEnd, lerp );

        if( Health <= 0 )
        {
            Instantiate( Explosion, transform.position, transform.rotation );
            Destroy( this.gameObject );
        }
    }

    void FixedUpdate()
    {
        // Only apply force to move if speed is lower than terminal.
        float speed = Vector3.Magnitude( rb.velocity );
        if( speed < ThrustSpeed )
        {
            rb.AddForce( currentVelocity );
        }
    }

    void OnCollisionEnter( Collision other )
    {
        if( other.gameObject.tag == "Character" )
        {
            Physics.IgnoreCollision( other.gameObject.GetComponent<Collider>(), GetComponent<Collider>() );
        }
    }


    public void Thrust( float amount )
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

    public void Rotate( float amount )
    {
        if( amount != 0 )
        {
            float rotation = -Mathf.Sign( amount );
            transform.rotation *= Quaternion.Euler( 0, 0, rotation * RotationSpeed * Time.deltaTime );
        }
    }

    public void FireStandard()
    {
        standardWeapon.Fire( transform.position, FrontVector, currentVelocity, Team );
    }

    public void FireHeavy()
    {
        if( !heavyWeapon )
            return;

        heavyWeapon.Fire( transform.position, FrontVector, currentVelocity, Team );
    }

    public void PickUpWeapon( WeaponTypes.Type type )
    {
        Weapon heavy = WeaponTypes.GetWeapon( type );
        heavyWeapon = ( Weapon ) Instantiate( heavy, transform.position, transform.rotation );
        heavyWeapon.transform.SetParent( transform );
    }
}
