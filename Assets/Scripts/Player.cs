using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Ship ship;

    public AudioSource ThrustSound;
    public AudioSource BounceSound;
    void Awake()
    {
        ship = GetComponent<Ship>();
    }

    // Use this for initialization
    void Start()
    {

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
        float fire2 = Input.GetAxis( "Fire2" );
        if( fire2 != 0 )
        {
            ship.FireHeavy();
        }
        else if( fire != 0 )
        {
            ship.FireStandard();
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
