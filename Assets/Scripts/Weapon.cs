using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public GameController Game;
    public Rigidbody Bullet;
    public string Name;
    public Color Colour;
    public float BulletSpeed;
    public float BulletDuration;
    public int BulletDamage;
    public float BulletSize;
    public int NumBullets;
    public float SpreadAngle;
    public bool IsRandomSpread;
    public float BulletsPerSecond;
    private float currentFireDelay;
    public int Ammo;
    public bool IsAffectedByShipVel;
    public float BlastSize;
    private AudioSource fireSound;
    public GameObject HitSpark;

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

    public bool CanFire()
    {
        return currentFireDelay == 0 && ( Ammo > 0 || Ammo == -1 );
    }

    public void Fire( Vector3 position, Vector3 direction, Vector3 currentVelocity, int team )
    {
        // -1 is infinite ammo.
        if( CanFire() )
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
                Vector3 thisDir = currentDir;
                if( IsRandomSpread )
                {
                    Quaternion rot = Quaternion.Euler( new Vector3( 0, Random.Range( -SpreadAngle, SpreadAngle ), 0 ) );
                    thisDir = rot * thisDir;
                }

                // Push the bullet's position forward by half its radius.
                Vector3 pos = position + thisDir.normalized * Bullet.transform.localScale.x * BulletSize / 2;
                Rigidbody bulletInstance = ( Rigidbody ) Instantiate( Bullet, pos, transform.rotation );

                // Apply bullet properties.
                bulletInstance.velocity = thisDir * BulletSpeed;
                if( IsAffectedByShipVel )
                    bulletInstance.velocity = bulletInstance.velocity + currentVelocity / 3;

                Bullet bullet = bulletInstance.GetComponent<Bullet>();
                bullet.Team = team;
                bullet.Damage = BulletDamage;
                bullet.GetComponent<Renderer>().material.SetColor( "_Color", Colour );
                bullet.transform.localScale = bullet.transform.localScale * BulletSize;
                bullet.HitSpark = HitSpark;
                bullet.BlastSize = BlastSize;
                bullet.Duration = BulletDuration;

                // Edit trail size and colour.
                TrailRenderer trail = bullet.GetComponent<TrailRenderer>();
                trail.material.color = Colour;
                AnimationCurve trailCurve = new AnimationCurve();
                trailCurve.AddKey( 0, BulletSize );
                trailCurve.AddKey( 1, 0 );
                trail.widthCurve = trailCurve;

                // Calculate the next bullet's direction.
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