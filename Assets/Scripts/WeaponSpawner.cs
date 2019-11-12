using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSpawner : MonoBehaviour
{
    public WeaponPickup WeaponPickup;
    private WeaponTypes weaponTypes;

    private Vector2 levelXBounds;
    private Vector2 levelZBounds;

    // Use this for initialization
    void Start()
    {
        weaponTypes = GetComponent<WeaponTypes>();
        levelXBounds = new Vector2( -18, 18 );
        levelZBounds = new Vector2( -18, 18 );

        // TODO: testing purposes, remove later.
        Spawn( WeaponTypes.Type.Blaster );
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void Spawn(WeaponTypes.Type type)
    {
        Weapon wpn = weaponTypes.GetWeapon( type );
        float x = Random.Range( levelXBounds.x, levelXBounds.y );
        float z = Random.Range( levelZBounds.x, levelZBounds.y );
        WeaponPickup pickup = ( WeaponPickup ) Instantiate( WeaponPickup, new Vector3(x, 0, z), transform.rotation );
        pickup.GetComponent<Renderer>().material.SetColor( "_Color", wpn.Colour );
    }
}
