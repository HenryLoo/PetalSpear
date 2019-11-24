using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Ship Ship;

    public AudioSource ThrustSound;
    public AudioSource BounceSound;
    public AudioSource RollSound;

    // Use this for initialization
    void Start()
    {
        Input.ResetInputAxes();
    }

    // Update is called once per frame
    void Update()
    {
        float thrust = Input.GetAxis( "Thrust" );
        Ship.Thrust( thrust );
        if( thrust != 0 && !ThrustSound.isPlaying )
            ThrustSound.Play();
        else if( thrust == 0 )
            ThrustSound.Stop();

        float rotate = Input.GetAxis( "Rotate" );
        Ship.Rotate( rotate );

        float fireStandard = Input.GetAxis( "Standard Weapon" );
        float fireHeavy = Input.GetAxis( "Heavy Weapon" );
        if( fireHeavy != 0 )
        {
            Ship.FireHeavy();
        }
        else if( fireStandard != 0 )
        {
            Ship.FireStandard();
        }

        if( Input.GetButtonDown( "Dodge Roll" ) )
        {
            if ( rotate < 0 )
            {
                Ship.DodgeRoll( true );
                RollSound.Play();
            }
            else if( rotate > 0 )
            {
                Ship.DodgeRoll( false );
                RollSound.Play();
            }
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
