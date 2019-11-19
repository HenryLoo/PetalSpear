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

    private const float RETRY_SEEK_PICKUP_DURATION = 3;
    private float retrySeekPickupTimer;

    private StateMachine states;

    private const string STATE_UNARMED = "Unarmed";
    private const string STATE_ARMED = "Armed";
    private const string GOAL_KILL_PLAYER = "KillPlayer";
    private const string GOAL_GET_ARMED = "GetArmed";
    private const string ACTION_PURSUE_PLAYER = "PursuePlayer";
    private const string ACTION_SEEK_PICKUP = "SeekPickup";

    private const int VALUE_KILL_PLAYER = 1;
    private const int VALUE_GET_ARMED = 0;
    private const int VALUE_GET_ARMED_PRIORITY = 2;

    private const int INSISTENCE_PURSUE_PLAYER = -1;
    private const int INSISTENCE_SEEK_PICKUP = -2;

    private Dictionary<string, AIBehaviour> actionToBehaviour = new Dictionary<string, AIBehaviour>
    {
        { ACTION_PURSUE_PLAYER, AIBehaviour.PursuePlayer },
        { ACTION_SEEK_PICKUP, AIBehaviour.SeekPickup },
    };

    private GoalOrientedBehaviour.OverallUtility gob;

    enum AIBehaviour
    {
        PursuePlayer,
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

        InitGOB();
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
                retrySeekPickupTimer = 0;
                targetPos = playerPos;
                FaceTarget();
                Arrive();
                Ship.Thrust( thrustAmount );
                break;
            }

            case AIBehaviour.SeekPickup:
            {
                // Slow down. This occurs at the beginning to realign and prepare
                // to move toward the weapon pickup. This also occurs again if
                // it takes too long to pick up the weapon, to break stalling.
                if( Vector3.Magnitude( rb.velocity ) >= 1 && retrySeekPickupTimer == 0 )
                {
                    float angleDiff = SignedAngle( Ship.FrontVector, -rb.velocity, Vector3.up );
                    if( Mathf.Abs( angleDiff ) > 1 )
                    {
                        float direction = Mathf.Sign( angleDiff );
                        Ship.Rotate( direction );
                    }
                    else
                    {
                        Ship.Thrust( 1 );
                    }
                    break;
                }
                // Try to pick up the weapon.
                else
                {
                    // Just starting to move toward pickup, so reinitialize thrust.
                    if( retrySeekPickupTimer == 0 )
                    {
                        Ship.Thrust( 0 );
                    }

                    // If stalling too long, we need to stop and realign to try again.
                    retrySeekPickupTimer += Time.deltaTime;
                    if( retrySeekPickupTimer >= RETRY_SEEK_PICKUP_DURATION )
                    {
                        retrySeekPickupTimer = 0;
                    }

                    targetPos = pickupPos;
                    FaceTarget();
                    float angleDiff = Vector3.Angle( Ship.FrontVector, thisToTarget );
                    if( angleDiff < 5 )
                    {
                        Ship.Thrust( 1 );
                    }
                    break;
                }
            }
        }

        // Don't waste ammo if the player is invincible.
        if( isFiringHeavy && Game.CurrentPlayer.InvincibilityTimer == 0)
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

    private void InitGOB()
    {

        // Initialize goal oriented behaviour handler.
        gob = new GoalOrientedBehaviour.OverallUtility();

        // Initialize goals.
        List<GoalOrientedBehaviour.Goal> goals = new List<GoalOrientedBehaviour.Goal>();

        // Goal: Kill player.
        GoalOrientedBehaviour.Goal killPlayer = new GoalOrientedBehaviour.Goal
        {
            Name = GOAL_KILL_PLAYER,
            Value = 0
        };
        goals.Add( killPlayer );

        // Goal: Get armed.
        GoalOrientedBehaviour.Goal getArmed = new GoalOrientedBehaviour.Goal
        {
            Name = GOAL_GET_ARMED,
            Value = 0
        };
        goals.Add( getArmed );

        gob.Goals = goals;

        // Initialize actions.
        List<GoalOrientedBehaviour.Action> actions = new List<GoalOrientedBehaviour.Action>();

        // Action: Pursue player.
        GoalOrientedBehaviour.Action pursuePlayer = new GoalOrientedBehaviour.Action
        {
            Name = ACTION_PURSUE_PLAYER,
            Insistences = new Dictionary<string, int>
            {
                { GOAL_KILL_PLAYER, INSISTENCE_PURSUE_PLAYER },
                { GOAL_GET_ARMED, 0 }
            }
        };
        actions.Add( pursuePlayer );

        // Action: Seek pickup.
        GoalOrientedBehaviour.Action seekPickup = new GoalOrientedBehaviour.Action
        {
            Name = ACTION_SEEK_PICKUP,
            Insistences = new Dictionary<string, int>
            {
                { GOAL_KILL_PLAYER, 0 },
                { GOAL_GET_ARMED, INSISTENCE_SEEK_PICKUP }
            }
        };
        actions.Add( seekPickup );

        gob.Actions = actions;
    }

    private void InitStateMachine()
    {
        // Initialize state machine's states.
        states = new StateMachine();

        // Unarmed state.
        StateMachine.State stateUnarmed = new StateMachine.State
        {
            EntryAction = () =>
            {
                // Change "kill player" goal value.
                GoalOrientedBehaviour.Goal killPlayer = new GoalOrientedBehaviour.Goal
                {
                    Name = GOAL_KILL_PLAYER,
                    Value = VALUE_KILL_PLAYER
                };
                gob.Goals[ 0 ] = killPlayer;
            },

            Action = () =>
            {
                // Change "get armed" goal value.
                int getArmedVal = Game.CurrentPickup ?
                    VALUE_GET_ARMED_PRIORITY : VALUE_GET_ARMED;
                if( getArmedVal != gob.Goals[ 1 ].Value )
                {
                    GoalOrientedBehaviour.Goal getArmed = new GoalOrientedBehaviour.Goal
                    {
                        Name = GOAL_GET_ARMED,
                        Value = getArmedVal
                    };
                    gob.Goals[ 1 ] = getArmed;
                }

                UpdateCurrentBehaviour();
            }
        };

        // Armed state.
        StateMachine.State stateArmed = new StateMachine.State
        {
            EntryAction = () =>
            {
                // Change "kill player" goal value.
                GoalOrientedBehaviour.Goal killPlayer = new GoalOrientedBehaviour.Goal
                {
                    Name = GOAL_KILL_PLAYER,
                    Value = VALUE_KILL_PLAYER
                };
                gob.Goals[ 0 ] = killPlayer;
            },

            Action = () =>
            {
                // Change "get armed" goal value.
                // Try to deny the player if they are unarmed.
                int getArmedVal = !Game.CurrentPlayer.HeavyWeapon ?
                    VALUE_GET_ARMED_PRIORITY : VALUE_GET_ARMED;

                if( getArmedVal != gob.Goals[ 1 ].Value )
                {
                    GoalOrientedBehaviour.Goal getArmed = new GoalOrientedBehaviour.Goal
                    {
                        Name = GOAL_GET_ARMED,
                        Value = getArmedVal
                    };
                    gob.Goals[ 1 ] = getArmed;
                }

                UpdateCurrentBehaviour();
            }
        };

        // Transition: unarmed to armed.
        StateMachine.Transition unarmedToArmed = new StateMachine.Transition()
        {
            TargetState = STATE_ARMED,
            IsTriggered = () =>
            {
                return Ship.HeavyWeapon;
            }
        };
        stateUnarmed.Transitions.Add( unarmedToArmed );

        // Transition: armed to unarmed.
        StateMachine.Transition armedToUnarmed = new StateMachine.Transition()
        {
            TargetState = STATE_UNARMED,
            IsTriggered = () =>
            {
                return !Ship.HeavyWeapon;
            }
        };
        stateArmed.Transitions.Add( armedToUnarmed );

        // Add states to the state machine.
        states.AddState( STATE_UNARMED, stateUnarmed );
        states.AddState( STATE_ARMED, stateArmed );
    }

    public void UpdateCurrentBehaviour()
    {
        GoalOrientedBehaviour.Action action = gob.ChooseAction();
        AIBehaviour behaviour;
        actionToBehaviour.TryGetValue( action.Name, out behaviour );
        currentBehaviour = behaviour;
    }
}
