using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Rigidbody Bullet;
    public string Name;
    public Color Colour;
    public float BulletSpeed;
    public int BulletDamage;
    public float BulletSize;
    public int NumBullets;
    public float SpreadAngle;
    public float BulletsPerSecond;
    private float currentFireDelay;
    private AudioSource fireSound;
    public int Ammo;

    void Awake()
    {
        fireSound = GetComponent<AudioSource>();
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateValues()
    {
        if( currentFireDelay > 0 )
        {
            currentFireDelay -= Time.deltaTime;
            currentFireDelay = Mathf.Max( currentFireDelay, 0 );
        }

        if( Ammo == 0 && !fireSound.isPlaying )
        {
            Destroy( this.gameObject );
        }
    }

    public void Fire( Vector3 position, Vector3 direction, Vector3 currentVelocity, int team )
    {
        // -1 is infinite ammo.
        if( currentFireDelay == 0 && ( Ammo > 0 || Ammo == -1 ) )
        {
            Vector3 currentDir = direction;
            if( NumBullets > 1 )
            {
                // Spread for even numbered bullets.
                if( NumBullets % 2 == 0 )
                    currentDir = Quaternion.Euler( 0, -SpreadAngle * ( NumBullets - 1 ) / 2, 0 ) * direction;
                // Spread for odd numbered bullets.
                else
                    currentDir = Quaternion.Euler( 0, -SpreadAngle * Mathf.Floor( NumBullets / 2 ), 0 ) * direction;
            }

            for( int i = 0; i < NumBullets; ++i )
            {
                Rigidbody bulletInstance = ( Rigidbody ) Instantiate( Bullet, position, transform.rotation );
                bulletInstance.velocity = currentDir * ( BulletSpeed + Vector3.Magnitude( currentVelocity ) );
                Bullet bullet = bulletInstance.GetComponent<Bullet>();
                bullet.Team = team;
                bullet.Damage = BulletDamage;
                bullet.GetComponent<Renderer>().material.SetColor( "_Color", Colour );
                bullet.transform.localScale = bullet.transform.localScale * BulletSize;
                TrailRenderer trail = bullet.GetComponent<TrailRenderer>();
                trail.material.color = Colour;
                trail.widthMultiplier = trail.widthMultiplier * BulletSize * 2;
                currentDir = Quaternion.Euler( 0, SpreadAngle, 0 ) * currentDir;
            }

            currentFireDelay = 1 / BulletsPerSecond;
            fireSound.Play();

            // Decrement ammo.
            if( Ammo != -1 )
                --Ammo;
        }
    }
}