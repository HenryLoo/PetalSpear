using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public GameController Game;
    public WeaponTypes.Type WeaponType;
    private AudioSource pickupSound;
    private bool isPickedUp = false;

    // Use this for initialization
    void Awake()
    {
        pickupSound = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        float rotationSpeed = 45 * Time.deltaTime;
        transform.rotation = Quaternion.Euler( 0, rotationSpeed, rotationSpeed ) * transform.rotation;

        if (isPickedUp && !pickupSound.isPlaying)
        {
            Destroy( this.gameObject );
        }
    }

    private void OnTriggerEnter( Collider other )
    {
        if( isPickedUp ) return;

        Ship otherShip = other.gameObject.GetComponent<Ship>();
        if( otherShip )
        {
            otherShip.PickUpWeapon( WeaponType );
            pickupSound.Play();
            isPickedUp = true;
            GetComponent<Renderer>().enabled = false;
            Game.ResetWeaponSpawnTimer();
        }
    }
}
