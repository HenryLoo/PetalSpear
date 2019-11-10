using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Ship ship;

    public AudioSource ThrustSound;
    public AudioSource BounceSound;

    // Use this for initialization
    void Start()
    {
        ship = GetComponent<Ship>();
    }

    // Update is called once per frame
    void Update()
    {
        float vertical = Input.GetAxis( "Vertical" );
        ship.Thrust( vertical );
        if( vertical != 0 && !ThrustSound.isPlaying )
            ThrustSound.Play();
        else if( vertical == 0 )
            ThrustSound.Stop();

        float horizontal = Input.GetAxis( "Horizontal" );
        ship.Rotate( horizontal );

        float fire = Input.GetAxis( "Fire1" );
        if( fire != 0 )
        {
            ship.Fire();
        }
    }
    void OnCollisionEnter( Collision other )
    {
        if( other.gameObject.tag == "Wall" )
        {
            BounceSound.Play();
        }
    }
}
