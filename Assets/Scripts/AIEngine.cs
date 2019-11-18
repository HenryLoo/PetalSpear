using System;
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
    Vector3 thisToTarget;
    private Rigidbody rb;

    public GameController Game;
    private Vector3 targetPos;
    private Vector3 playerPos;
    private Vector3 pickupPos;
    //private float playerRotation;

    private bool isFiringStandard = false;
    private bool isFiringHeavy = false;
    public float FiringRadius = 90;

    private const float RETRY_SEEK_PICKUP_DURATION = 5;
    private float retrySeekPickupTimer;

    private StateMachine states;
    private const string STATE_PURSUE_PLAYER = "PursuePlayer";
    private const string STATE_SLOW_DOWN = "SlowDown";
    private const string STATE_SEEK_PICKUP = "SeekPickup";

    private GoalOrientedBehaviour.OverallUtility gob;

    enum AIBehaviour
    {
        PursuePlayer,
        SlowDown,
        SeekPickup
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
        rb = Ship.GetComponent<Rigidbody>();

        InitStateMachine();
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
        // Get player data.
        if( Game.CurrentPlayer )
        {
            Vector3 playerVel = Game.CurrentPlayer.GetVelocity() * Time.deltaTime;
            playerPos = Game.CurrentPlayer.transform.position + playerVel;
            //playerRotation = PlayerShip.transform.rotation.eulerAngles.y;
        }

        // Get pickup data.
        if( Game.CurrentPickup )
        {
            pickupPos = Game.CurrentPickup.transform.position;
        }
    }

    private void MakeDecision()
    {
        // Update the state machine and perform any available actions.
        List<Action> actions = states.Update();
        foreach( Action action in actions )
        {
            action();
        }

        // Calculate the vector to the target.
        thisToTarget = targetPos - transform.position;

        // Fire weapons if available.
        isFiringHeavy = Ship.HeavyWeapon ? IsFiring( Ship.HeavyWeapon ) : false;
        isFiringStandard = IsFiring( Ship.StandardWeapon );
    }

    private void Move()
    {
        switch( currentBehaviour )
        {
            case AIBehaviour.PursuePlayer:
            {
                FaceTarget();
                Arrive();
                Ship.Thrust( thrustAmount );
                break;
            }

            case AIBehaviour.SlowDown:
            {
                float angleDiff = SignedAngle( Ship.FrontVector, -rb.velocity, Vector3.up );
                if( Mathf.Abs( angleDiff ) > 1 )
                {
                    float direction = Mathf.Sign( angleDiff );
                    Ship.Rotate( direction );
                }
                else if( Vector3.Magnitude( rb.velocity ) > 0 )
                {
                    Ship.Thrust( 1 );
                }
                break;
            }

            case AIBehaviour.SeekPickup:
            {
                FaceTarget();
                float angleDiff = Vector3.Angle( Ship.FrontVector, thisToTarget );
                if( angleDiff < 5 )
                {
                    Ship.Thrust( 1 );
                }
                break;
            }
        }

        if( isFiringHeavy )
        {
            Ship.FireHeavy();
        }
        else if( isFiringStandard )
        {
            Ship.FireStandard();
        }
    }

    // Adjust thrust amount based on distance to player.
    private void Arrive()
    {
        float dist = Vector3.Magnitude( thisToTarget );
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

    private void FaceTarget()
    {
        float angleDiff = SignedAngle( Ship.FrontVector, thisToTarget, Vector3.up );
        float direction = Mathf.Sign( angleDiff );
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

    private bool IsFiring( Weapon wpn )
    {
        bool isFiring = Game.CurrentPlayer;
        Vector3 thisToPlayer = playerPos - transform.position;
        isFiring &= Vector3.Angle( Ship.FrontVector, thisToPlayer ) <= FiringRadius;
        float bulletRange = wpn.BulletSpeed * wpn.BulletDuration;
        isFiring &= Vector3.Magnitude( thisToPlayer ) <= bulletRange;
        return isFiring;
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

    private void InitStateMachine()
    {
        // Initialize state machine's states.
        states = new StateMachine();

        // Pursue player.
        StateMachine.State statePursue = new StateMachine.State
        {
            EntryAction = () =>
            {
                currentBehaviour = AIBehaviour.PursuePlayer;
            },

            Action = () =>
            {
                targetPos = playerPos;
            }
        };

        // Slow down.
        StateMachine.State stateSlowDown = new StateMachine.State
        {
            EntryAction = () =>
            {
                currentBehaviour = AIBehaviour.SlowDown;
                retrySeekPickupTimer = 0;
            }
        };
        
        // Seek pickup.
        StateMachine.State statePickup = new StateMachine.State
        {
            EntryAction = () =>
            {
                currentBehaviour = AIBehaviour.SeekPickup;
            },

            Action = () =>
            {
                targetPos = pickupPos;
                retrySeekPickupTimer += Time.deltaTime;
            }
        };

        // Transition: Pursue player to slow down.
        StateMachine.Transition pursueToSlowDown = new StateMachine.Transition
        {
            IsTriggered = () =>
            {
                return Game.CurrentPickup;
            },
            TargetState = STATE_SLOW_DOWN
        };
        statePursue.Transitions.Add( pursueToSlowDown );

        // Transition: Slow down to pickup.
        StateMachine.Transition slowDownToPickup = new StateMachine.Transition
        {
            IsTriggered = () =>
            {
                return Vector3.Magnitude( rb.velocity ) < 1;
            },
            Action = () =>
            {
                Ship.Thrust( 0 );
            },
            TargetState = STATE_SEEK_PICKUP
        };
        stateSlowDown.Transitions.Add( slowDownToPickup );

        // Transition: Pickup was taken by the player.
        StateMachine.Transition noPickup = new StateMachine.Transition
        {
            IsTriggered = () =>
            {
                return !Game.CurrentPickup;
            },
            TargetState = STATE_PURSUE_PLAYER
        };
        stateSlowDown.Transitions.Add( noPickup );
        statePickup.Transitions.Add( noPickup );

        // Transition: Retry seek pickup if timed out.
        StateMachine.Transition retrySeekPickup = new StateMachine.Transition
        {
            IsTriggered = () =>
            {
                return retrySeekPickupTimer >= RETRY_SEEK_PICKUP_DURATION;
            },
            TargetState = STATE_SLOW_DOWN
        };
        statePickup.Transitions.Add( retrySeekPickup );

        // Add the states to the state machine.
        states.AddState( STATE_PURSUE_PLAYER, statePursue );
        states.AddState( STATE_SLOW_DOWN, stateSlowDown );
        states.AddState( STATE_SEEK_PICKUP, statePickup );
    }
}
