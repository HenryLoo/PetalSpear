using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float RotationSpeed = 180;
    public float ThrustSpeed = 10;

    public Rigidbody Bullet;
    public float BulletSpeed;

    private Rigidbody rb;
    private Vector3 currentVelocity;

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float vertical = Input.GetAxis( "Vertical" );
        if( vertical != 0 )
        {
            float thrust = Mathf.Sign( vertical );
            currentVelocity = transform.up * thrust * ThrustSpeed;
        }
        else
        {
            currentVelocity = new Vector3( 0, 0, 0 );
        }

        float horizontal = Input.GetAxis( "Horizontal" );
        if( horizontal != 0 )
        {
            float rotation = -Mathf.Sign( horizontal );
            transform.rotation *= Quaternion.Euler( 0, 0, rotation * RotationSpeed * Time.deltaTime );
        }

        float fire = Input.GetAxis( "Fire1" );
        if( fire != 0 )
        {
            Rigidbody bulletInstance = ( Rigidbody ) Instantiate( Bullet, transform.position, transform.rotation );
            bulletInstance.velocity = transform.up * BulletSpeed;
        }
    }

    void FixedUpdate()
    {
        rb.AddForce( currentVelocity );
    }
}
