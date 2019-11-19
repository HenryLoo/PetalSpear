using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
    public int Health = 100;

    public int Team;

    public float RotationSpeed = 180;
    public float ThrustSpeed = 10;

    public WeaponTypes WeaponTypes;
    public Weapon StandardWeapon;
    public Weapon HeavyWeapon;

    private Rigidbody rb;
    private Vector3 currentVelocity;

    public Vector3 FrontVector;

    public GameObject Explosion;

    private TrailRenderer trail;

    private Renderer rend;
    private Color colourStart = Color.white;
    private Color colourEnd = Color.white;

    public TextMesh Text;
    private const float PICKUP_TEXT_DURATION = 2;

    public float InvincibilityTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        trail = GetComponent<TrailRenderer>();
        rend = GetComponent<Renderer>();
    }

    // Use this for initialization
    void Start()
    {
        Weapon standard = WeaponTypes.GetWeapon( WeaponTypes.Type.Standard );
        StandardWeapon = ( Weapon ) Instantiate( standard, transform.position, transform.rotation );
        StandardWeapon.transform.SetParent( transform );

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
        FrontVector = transform.up;

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
        if( amount != 0 )
        {
            float rotation = -Mathf.Sign( amount );
            transform.rotation *= Quaternion.Euler( 0, 0, rotation * RotationSpeed * Time.deltaTime );
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
}
