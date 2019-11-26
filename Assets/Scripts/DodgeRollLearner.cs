using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DodgeRollLearner : MonoBehaviour
{
    public GameController Game;
    public float Duration;
    private bool isDone = false;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Duration -= Time.deltaTime;
        if( Duration <= 0 )
        {
            // Timed out and player didn't dodge.
            Game.UpdateNBayes( false );
            isDone = true;
        }

        bool isPlayerExist = Game.CurrentPlayer;
        if( isPlayerExist && Game.CurrentPlayer.IsRolling() )
        {
            // Player dodged.
            Game.UpdateNBayes(true);
            isDone = true;
        }

        // NBayes example was found, so destroy this.
        if( isDone )
            Destroy( gameObject );
    }
}
