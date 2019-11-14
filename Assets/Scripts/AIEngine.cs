using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIEngine : MonoBehaviour
{
    public float MaxSpeedRadius = 30;
    public float ArrivalRadius = 10;
    public float ThrustGain = 0.5f;

    public Ship Ship;
    private float thrustAmount;
    Vector3 thisToPlayer;

    public GameController Game;
    private Vector3 playerPos;
    //private float playerRotation;

    private bool isFiring = false;
    public float FiringRadius = 45;

    enum AIBehaviour
    {
        PursuePlayer
    }

    enum HealthState
    {
        Healthy,
        Hurt,
        Dead
    }

    private AIBehaviour currentBehaviour;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        GetWorldData();
        MakeDecision();
        Move();
    }

    private void GetWorldData()
    {
        if( Game.CurrentPlayer )
        {
            playerPos = Game.CurrentPlayer.transform.position;
            //playerRotation = PlayerShip.transform.rotation.eulerAngles.y;
        }

        thisToPlayer = playerPos - transform.position;
    }

    private void MakeDecision()
    {
        if( ClassifyHealth( Ship.Health ) == HealthState.Healthy )
        {
            currentBehaviour = AIBehaviour.PursuePlayer;
        }

        IsFiring();
    }

    private void Move()
    {
        switch( currentBehaviour )
        {
            case AIBehaviour.PursuePlayer:
            {
                FacePlayer();
                Arrive();
                Ship.Thrust( thrustAmount );
                break;
            }
        }

        if( isFiring )
            Ship.FireStandard();
    }

    // Adjust thrust amount based on distance to player.
    private void Arrive()
    {
        float dist = Vector3.Magnitude( thisToPlayer );
        float targetThrustAmount = 0;
        if( dist > MaxSpeedRadius )
            targetThrustAmount = 1.0f;
        else if( dist > ArrivalRadius )
        {
            targetThrustAmount = ( dist - ArrivalRadius ) / ( MaxSpeedRadius - ArrivalRadius );
        }


        // Adjust to match target thrust.
        if( thrustAmount < targetThrustAmount )
            thrustAmount += ThrustGain * Time.deltaTime;
        else if( thrustAmount > targetThrustAmount )
            thrustAmount -= ThrustGain * Time.deltaTime;
    }

    private void FacePlayer()
    {
        float direction = 0;
        float angleDiff = SignedAngle( Ship.FrontVector, thisToPlayer, Vector3.up );
        direction = Mathf.Sign( angleDiff );
        Ship.Rotate( direction );
    }

    private HealthState ClassifyHealth( float health )
    {
        if( health == 0 )
            return HealthState.Dead;
        else if( health <= 60 )
            return HealthState.Hurt;
        else
            return HealthState.Healthy;
    }

    private void IsFiring()
    {
        isFiring = Game.CurrentPlayer;
        isFiring &= Vector3.Angle( Ship.FrontVector, thisToPlayer ) <= FiringRadius;
        float bulletRange = Ship.StandardWeapon.BulletSpeed * Ship.StandardWeapon.BulletDuration;
        isFiring &= Vector3.Magnitude( thisToPlayer ) <= bulletRange;
    }

    // SignedAngle doesn't exist in this version of Unity.
    public static float SignedAngle( Vector3 from, Vector3 to, Vector3 axis )
    {
        float unsignedAngle = Vector3.Angle( from, to );

        float cross_x = from.y * to.z - from.z * to.y;
        float cross_y = from.z * to.x - from.x * to.z;
        float cross_z = from.x * to.y - from.y * to.x;
        float sign = Mathf.Sign( axis.x * cross_x + axis.y * cross_y + axis.z * cross_z );
        return unsignedAngle * sign;
    }
}
