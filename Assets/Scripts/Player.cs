using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Ship ship;

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

        float horizontal = Input.GetAxis( "Horizontal" );
        ship.Rotate( horizontal );

        float fire = Input.GetAxis( "Fire1" );
        if( fire != 0 )
        {
            ship.Fire();
        }
    }
}
