using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
    public GameController Game;

    public int Health = 100;

    public int Team;

    public float RotationSpeed = 180;
    public float ThrustSpeed = 10;
    private float DodgeSpeed;
    private const float DODGE_SPEED_MULTIPLIER = 40;

    public WeaponTypes WeaponTypes;
    public Weapon StandardWeapon;
    public Weapon HeavyWeapon;

    private Rigidbody rb;
    private Vector3 currentVelocity;
    private float currentRotationDirection;

    public Vector3 FrontVector;

    public GameObject Explosion;

    private TrailRenderer trail;

    private Renderer rend;
    private Color colourStart = Color.white;
    private Color colourEnd = Color.white;

    public TextMesh Text;
    private const float PICKUP_TEXT_DURATION = 2;

    public float InvincibilityTimer;

    private Animator animator;

    private bool isRollingLeft;
    private bool isRollingRight;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        trail = GetComponentInChildren<TrailRenderer>();
        rend = GetComponentInChildren<Renderer>();
        animator = GetComponentInChildren<Animator>();
    }

    // Use this for initialization
    void Start()
    {
        DodgeSpeed = ThrustSpeed * DODGE_SPEED_MULTIPLIER;

        Weapon standard = WeaponTypes.GetWeapon( WeaponTypes.Type.Standard );
        StandardWeapon = ( Weapon ) Instantiate( standard, transform.position, transform.rotation );
        StandardWeapon.transform.SetParent( transform );
        StandardWeapon.Game = Game;

        // Initialize material values.
        rend.material.SetFloat( "_Mode", 4f );
        rend.material.SetInt( "_SrcBlend", ( int ) UnityEngine.Rendering.BlendMode.SrcAlpha );
        rend.material.SetInt( "_DstBlend", ( int ) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha );
        rend.material.SetInt( "_ZWrite", 0 );
        rend.material.DisableKeyword( "_ALPHATEST_ON" );
        rend.material.EnableKeyword( "_ALPHABLEND_ON" );
        rend.material.DisableKeyword( "_ALPHAPREMULTIPLY_ON" );
        rend.material.renderQueue = 3000;
    }

    // Update is called once per frame
    void Update()
    {
        FrontVector = transform.forward;

        trail.enabled = ( currentVelocity != Vector3.zero );

        StandardWeapon.UpdateValues();

        if( HeavyWeapon )
        {
            HeavyWeapon.UpdateValues();
            colourEnd = HeavyWeapon.Colour;
        }
        else
        {
            colourEnd = Color.white;
        }

        float lerp = Mathf.PingPong( Time.time, 1 ) / 1;
        rend.material.color = Color.Lerp( colourStart, colourEnd, lerp );

        if( Health <= 0 )
        {
            Die();
        }

        if( InvincibilityTimer > 0 )
        {
            InvincibilityTimer -= Time.deltaTime;
            SetAlpha( 0.5f );
        }
        else
        {
            SetAlpha( 1 );
            InvincibilityTimer = 0;
        }
    }

    void FixedUpdate()
    {
        // Only apply force to move if speed is lower than terminal.
        float speed = Vector3.Magnitude( rb.velocity );
        if( speed < ThrustSpeed )
        {
            rb.AddForce( currentVelocity );
        }

        // Apply force on first frame of roll.
        if( isRollingLeft )
        {
            rb.velocity = Vector3.zero;
            rb.AddForce( -transform.right * DodgeSpeed );
            isRollingLeft = false;
        }
        else if( isRollingRight )
        {
            rb.velocity = Vector3.zero;
            rb.AddForce( transform.right * DodgeSpeed );
            isRollingRight = false;
        }
    }

    public void Thrust( float amount )
    {
        if( amount != 0 )
        {
            float thrust = Mathf.Sign( amount );
            currentVelocity = FrontVector * thrust * ThrustSpeed;
        }
        else
        {
            currentVelocity = new Vector3( 0, 0, 0 );
        }

    }

    public void Rotate( float amount )
    {
        currentRotationDirection = amount == 0 ? 0 : Mathf.Sign( amount );
        if( amount != 0 && !IsRolling() )
        {
            transform.rotation *= Quaternion.Euler( 0, currentRotationDirection * RotationSpeed * Time.deltaTime, 0 );
        }
    }

    public void FireStandard()
    {
        StandardWeapon.Fire( transform.position, FrontVector, currentVelocity, Team );
    }

    public void FireHeavy()
    {
        if( !HeavyWeapon )
            return;

        HeavyWeapon.Fire( transform.position, FrontVector, currentVelocity, Team );
    }

    public void PickUpWeapon( WeaponTypes.Type type )
    {
        // Replace existing weapon.
        if( HeavyWeapon )
            Destroy( HeavyWeapon.gameObject );

        Weapon heavy = WeaponTypes.GetWeapon( type );
        HeavyWeapon = ( Weapon ) Instantiate( heavy, transform.position, transform.rotation );
        HeavyWeapon.transform.SetParent( transform );
        HeavyWeapon.Game = Game;

        TextMesh text = Instantiate( Text, transform.position, Quaternion.Euler( new Vector3( 90, 0, 0 ) ) );
        text.text = HeavyWeapon.Name + " (" + HeavyWeapon.Ammo + "x)";
        text.GetComponent<TextDestroy>().Duration = PICKUP_TEXT_DURATION;
    }

    public Vector3 GetVelocity()
    {
        return rb.velocity;
    }

    private void SetAlpha( float a )
    {
        Color newCol = rend.material.color;
        newCol.a = a;
        rend.material.color = newCol;
    }
    public void Die()
    {
        Instantiate( Explosion, transform.position, transform.rotation );
        Destroy( this.gameObject );
    }

    public bool DodgeRoll()
    {
        // Already rolling.
        if( IsRolling() || currentRotationDirection == 0 )
            return false;

        if( currentRotationDirection < 0 )
        {
            animator.Play( "RollLeft" );
            isRollingLeft = true;
        }
        else if( currentRotationDirection > 0 )
        {
            animator.Play( "RollRight" );
            isRollingRight = true;
        }

        InvincibilityTimer = 0.2f;
        return true;
    }

    public bool IsRolling()
    {
        return IsRollingLeft() || IsRollingRight();
    }

    private bool IsRollingLeft()
    {
        return animator.GetCurrentAnimatorStateInfo( 0 ).IsName( "RollLeft" );
    }

    private bool IsRollingRight()
    {
        return animator.GetCurrentAnimatorStateInfo( 0 ).IsName( "RollRight" );
    }
}
