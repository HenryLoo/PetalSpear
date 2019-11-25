using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public GameController Game;
    public GameObject HitSpark;
    public BlastZone BlastZone;

    public int Team;
    public int Damage;
    public float BlastSize;
    public float Duration;
    private float currentDuration;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        currentDuration += Time.deltaTime;
        if (currentDuration >= Duration)
        {
            if( TriggerBlast() )
                ProcessHitSpark();
            Destroy( this.gameObject );
        }
    }

    private void OnTriggerEnter( Collider other )
    {
        if( other.gameObject.tag == "Wall" )
        {
            TriggerBlast();
            ProcessHitSpark();
            return;
        }

        Ship otherShip = other.gameObject.GetComponent<Ship>();
        if( otherShip && otherShip.Team != Team)
        {
            // If the player was hit, try to learn dodging.
            if( Game )
            {
                Player player = otherShip.GetComponentInChildren<Player>();
                Game.UpdateNBayes( player && otherShip.IsRolling() );
            }
            
            if( otherShip.InvincibilityTimer == 0 )
            {
                // If explosive, don't deal damage on contact.
                if( !TriggerBlast() )
                    otherShip.Health -= Damage;
                ProcessHitSpark();
            }

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

        BlastZone blast = (BlastZone) Instantiate( BlastZone, transform.position, transform.rotation );
        blast.Team = Team;
        blast.Damage = Damage;
        blast.gameObject.transform.localScale = new Vector3( BlastSize, BlastSize, BlastSize );
        return true;
    }
}