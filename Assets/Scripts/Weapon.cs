using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Rigidbody Bullet;
    public float BulletSpeed;
    public int BulletDamage;
    public float BulletsPerSecond;
    private float currentFireDelay;

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
    }

    public void Fire(Vector3 position, Vector3 direction, Vector3 currentVelocity, int team)
    {
        if( currentFireDelay == 0 )
        {
            Rigidbody bulletInstance = ( Rigidbody ) Instantiate( Bullet, position, transform.rotation );
            bulletInstance.velocity = direction * ( BulletSpeed + Vector3.Magnitude( currentVelocity ) );
            Bullet bullet = bulletInstance.GetComponent<Bullet>();
            bullet.Team = team;
            bullet.Damage = BulletDamage;
            currentFireDelay = 1 / BulletsPerSecond;
        }
    }
}