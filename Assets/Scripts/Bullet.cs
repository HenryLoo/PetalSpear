using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public GameObject HitSpark;
    public BlastZone BlastZone;

    public int Team;
    public int Damage;
    public float BlastSize;
    public float Duration;
    protected float currentDuration;

    private bool isDead;
    protected Rigidbody rb;
    private Renderer rend;

    protected void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponentInChildren<Renderer>();
    }

    // Use this for initialization
    protected void Start()
    {
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
    protected void Update()
    {
        currentDuration += Time.deltaTime;
        if( currentDuration >= Duration )
        {
            if( !isDead )
            {
                // Explode if duration has run out and this bullet has blast.
                if( TriggerBlast() )
                    ProcessHitSpark();
                // Otherwise fade out.
                else
                {
                    isDead = true;
                    rb.velocity = Vector3.zero;
                }
            }
            else
            {
                // Fade out if dead.
                Color newCol = rend.material.color;
                newCol.a -= ( 1.0f * Time.deltaTime );
                newCol.a = Mathf.Max( newCol.a, 0 );
                rend.material.color = newCol;

                if( newCol.a == 0 )
                    Destroy( this.gameObject );
            }
        }
    }

    protected void OnTriggerEnter( Collider other )
    {
        if( isDead )
            return;

        if( other.gameObject.tag == "Wall" )
        {
            TriggerBlast();
            ProcessHitSpark();
            return;
        }

        Ship otherShip = other.gameObject.GetComponent<Ship>();
        if( otherShip && otherShip.Team != Team &&
            otherShip.InvincibilityTimer == 0 )
        {
            // If explosive, don't deal damage on contact.
            if( !TriggerBlast() )
                otherShip.Health -= Damage;
            ProcessHitSpark();
            return;
        }
    }

    private void ProcessHitSpark()
    {
        Instantiate( HitSpark, transform.position, transform.rotation );
        Destroy( this.gameObject );
    }

    private bool TriggerBlast()
    {
        if( BlastSize == 0 )
            return false;

        BlastZone blast = ( BlastZone ) Instantiate( BlastZone, transform.position, transform.rotation );
        blast.Team = Team;
        blast.Damage = Damage;
        blast.gameObject.transform.localScale = new Vector3( BlastSize, BlastSize, BlastSize );
        return true;
    }
}