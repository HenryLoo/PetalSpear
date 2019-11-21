using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public Vector2 LevelXBounds;
    public Vector2 LevelZBounds;

    public Vector2 PlayerSpawnPos;
    public float PlayerSpawnRot = 0;
    public Vector2 OpponentSpawnPos;
    public float OpponentSpawnRot = 180;

    public float ShipSpawnDelay;
    public Vector2 WeaponSpawnRateRange;

    // Distance from a ship to spawn the weapon (when not random).
    public Vector2 WeaponSpawnDistRange;
    public float WeaponHelpDist;

    public float GameDuration;
    public float InvincibilityDuration;

    private float playerSpawnTimer;
    private float opponentSpawnTimer;
    private bool isReadyToSpawnPlayer = true;
    private bool isReadyToSpawnOpponent = true;
    public GameObject SpawnWarp;

    public WeaponPickup WeaponPickup;
    private WeaponTypes weaponTypes;
    private float weaponSpawnTimer;
    private bool isReadyToSpawnWeapon = true;

    public CameraFollow GameCamera;
    public Ship Ship;
    public Player Player;
    public AIEngine AIEngine;

    public Ship CurrentPlayer;
    private Ship currentOpponent;
    public WeaponPickup CurrentPickup;

    public TextMesh RedText;
    private const string DESTROYED_TEXT = "DESTROYED";
    private TextMesh playerDestroyed;
    private TextMesh opponentDestroyed;
    private Vector3 playerPos;
    private Vector3 opponentPos;

    public Canvas TitleUI;

    public Canvas GameUI;
    public Text TimeText;
    public Text PlayerScoreText;
    public Text OpponentScoreText;
    public Text PlayerWeaponText;
    public Text OpponentWeaponText;
    private float gameTime;
    private int playerScore;
    private int opponentScore;
    public RectTransform PlayerHealth;
    public RectTransform OpponentHealth;
    public float maxBarWidth;

    public Canvas EndUI;
    public Text WinText;
    private const string PLAYER_WIN = "PLAYER WINS!";
    private const string OPPONENT_WIN = "AI WINS!";
    private const string NO_WIN = "TIE GAME!";
    private AudioSource endSound;

    private GameState currentState = GameState.Title;

    private NGramPredictor pickupNGram;
    private const int N_GRAM_WINDOW_SIZE = 2;
    private const string N_GRAM_PLAYER_ACTION = "player";
    private const string N_GRAM_OPPONENT_ACTION = "ai";
    private List<string> pickupNGramSequence;
    private string nextPickupAction = string.Empty;

    enum GameState
    {
        Title,
        Playing,
        Ended
    }

    void Awake()
    {
        weaponTypes = GetComponent<WeaponTypes>();
        GameUI.enabled = false;
        EndUI.enabled = false;
        endSound = GetComponent<AudioSource>();
    }

    // Use this for initialization
    void Start()
    {
        maxBarWidth = PlayerHealth.rect.width;
        pickupNGram = new NGramPredictor( N_GRAM_WINDOW_SIZE );
        pickupNGramSequence = new List<string>();
    }

    // Update is called once per frame
    void Update()
    {
        // Title screen.
        switch(currentState)
        {
            case GameState.Title:
            {
                UpdateTitle();
                break;
            }

            case GameState.Playing:
            {
                UpdatePlaying();
                break;
            }

            case GameState.Ended:
            {
                UpdateEnded();
                break;
            }
        }
    }

    private void UpdateTitle()
    {
        // Slowly rotate camera.
        GameCamera.transform.rotation *= Quaternion.Euler( new Vector3( 0, 0, Time.deltaTime ) );

        bool fire = Input.GetButtonDown( "Fire1" );
        if( fire )
            StartGame();
    }

    private void UpdatePlaying()
    {
        // Update health bars.
        UpdateHealthBars();

        // Update weapon text.
        UpdateWeaponText();

        // Update game time.
        UpdateTime();

        if( !CurrentPickup )
        {
            if( isReadyToSpawnWeapon )
            {
                ResetWeaponSpawnTimer();
                isReadyToSpawnWeapon = false;
            }

            if( weaponSpawnTimer > 0 )
            {
                weaponSpawnTimer -= Time.deltaTime;
            }
            else
            {
                SpawnWeapon();
            }
        }

        if( CurrentPlayer )
        {
            playerPos = CurrentPlayer.transform.position;
        }
        else
        {
            if( isReadyToSpawnPlayer )
            {
                playerSpawnTimer = ShipSpawnDelay;
                isReadyToSpawnPlayer = false;
                playerDestroyed = CreateRedText( playerPos );

                // Player just died.
                ++opponentScore;
                UpdateScore();
            }

            if( playerSpawnTimer > 0 )
            {
                playerSpawnTimer -= Time.deltaTime;
                playerDestroyed.text = DESTROYED_TEXT + " (" + playerSpawnTimer.ToString( "F2" ) + " s)";
            }
            else
            {
                SpawnPlayer( true );
                isReadyToSpawnPlayer = true;
            }
        }

        if( currentOpponent )
        {
            opponentPos = currentOpponent.transform.position;
        }
        else
        {
            if( isReadyToSpawnOpponent )
            {
                opponentSpawnTimer = ShipSpawnDelay;
                isReadyToSpawnOpponent = false;
                opponentDestroyed = CreateRedText( opponentPos );

                // Opponent just died.
                ++playerScore;
                UpdateScore();
            }

            if( opponentSpawnTimer > 0 )
            {
                opponentSpawnTimer -= Time.deltaTime;
                opponentDestroyed.text = DESTROYED_TEXT + " (" + opponentSpawnTimer.ToString( "F2" ) + " s)";
            }
            else
            {
                SpawnOpponent( true );
                isReadyToSpawnOpponent = true;
            }
        }
    }

    private void UpdateEnded()
    {
        bool fire = Input.GetButtonDown( "Fire1" );
        if( fire )
            ResetTitle();
    }

    private void ResetTitle()
    {
        currentState = GameState.Title;

        // Show title screen.
        TitleUI.enabled = true;

        // Hide game UI.
        GameUI.enabled = false;

        // Hide end UI.
        EndUI.enabled = false;

        // Delete existing objects.
        if( currentOpponent )
            Destroy( currentOpponent.gameObject );
        if( CurrentPlayer )
            Destroy( CurrentPlayer.gameObject );
        if( CurrentPickup )
            Destroy( CurrentPickup.gameObject );
    }

    private void StartGame()
    {
        currentState = GameState.Playing;

        // Hide title screen.
        TitleUI.enabled = false;

        // Show game UI.
        GameUI.enabled = true;

        // Reset scores.
        playerScore = 0;
        opponentScore = 0;
        UpdateScore();

        // Reset timers.
        ResetWeaponSpawnTimer();
        gameTime = GameDuration;

        // Spawn ships.
        SpawnPlayer( false );
        SpawnOpponent( false );

        // Reset camera rotation.
        GameCamera.transform.rotation = Quaternion.Euler( 90, 0, 0 );
    }

    private void SpawnWeapon()
    {
        // A weapon already exists, so don't spawn another one.
        if( CurrentPickup )
            return;

        // Try to predict which ship will pick up this item.
        // To help the "losing" ship, spawn it closer to the ship that is
        // predicted to not pick it up.
        Vector3 position = GetRandomPosition();

        if( pickupNGramSequence.Count == N_GRAM_WINDOW_SIZE )
        {
            if ( nextPickupAction == string.Empty )
                nextPickupAction = pickupNGram.PredictNextAction( pickupNGramSequence );

            // A ship is being helped...
            bool isHelpingPlayer = ( nextPickupAction == N_GRAM_OPPONENT_ACTION && CurrentPlayer );
            bool isHelpingOpponent = ( nextPickupAction == N_GRAM_PLAYER_ACTION && currentOpponent );
            if( nextPickupAction != string.Empty )
            {
                bool tooClose = currentOpponent && CurrentPlayer &&
                    Vector3.Distance( currentOpponent.transform.position,
                    CurrentPlayer.transform.position ) < WeaponHelpDist;

                if( !tooClose && ( isHelpingPlayer || isHelpingOpponent ) )
                {
                    // Calculate a random distance from the ship being helped.
                    float x = UnityEngine.Random.Range( WeaponSpawnDistRange.x, WeaponSpawnDistRange.y );
                    float z = UnityEngine.Random.Range( WeaponSpawnDistRange.x, WeaponSpawnDistRange.y );
                    Vector3 weaponDist = new Vector3( x, 0, z );

                    position = ( nextPickupAction == N_GRAM_PLAYER_ACTION ) ?
                        currentOpponent.transform.position :
                        CurrentPlayer.transform.position;
                    position += weaponDist;

                    // Make sure the weapon stays within the level bounds.
                    position.x = Mathf.Clamp( position.x, LevelXBounds.x, LevelXBounds.y );
                    position.z = Mathf.Clamp( position.z, LevelZBounds.x, LevelZBounds.y );

                    // Reset the next pickup action.
                    nextPickupAction = string.Empty;
                }
                else
                {
                    // We want to help a ship, but that ship is not in a 
                    // position to be helped (either too close to its enemy
                    // or the ship we want to help is dead).
                    // So delay this weapon spawn.
                    return;
                }
            }
        }

        Weapons wpn = weaponTypes.GetRandomWeapon();
        WeaponPickup pickup = ( WeaponPickup ) Instantiate( WeaponPickup, position, transform.rotation );
        pickup.Game = this;
        pickup.WeaponType = wpn.Type;
        pickup.GetComponent<Renderer>().material.SetColor( "_Color", wpn.Weapon.Colour );
        CurrentPickup = pickup;

        // Set the flag to confirm weapon has been spawned.
        isReadyToSpawnWeapon = true;
    }

    private void SpawnPlayer( bool isRandomPos )
    {
        // Player already exists, so don't spawn another one.
        if( CurrentPlayer )
            return;

        CurrentPlayer = SpawnShip( isRandomPos, 0, PlayerSpawnPos, PlayerSpawnRot );
        Player player = ( Player ) Instantiate( Player,
            CurrentPlayer.transform.position, CurrentPlayer.transform.rotation );
        player.transform.SetParent( CurrentPlayer.transform );
        player.Ship = CurrentPlayer;
        GameCamera.Target = CurrentPlayer.transform;
    }

    private void SpawnOpponent( bool isRandomPos )
    {
        // Opponent already exists, so don't spawn another one.
        if( currentOpponent )
            return;

        currentOpponent = SpawnShip( isRandomPos, 1, OpponentSpawnPos, OpponentSpawnRot );
        AIEngine opponent = ( AIEngine ) Instantiate( AIEngine,
            currentOpponent.transform.position, currentOpponent.transform.rotation );
        opponent.transform.SetParent( currentOpponent.transform );
        opponent.Game = this;
        opponent.Ship = currentOpponent;
    }

    private Ship SpawnShip( bool isRandomPos, int team, Vector2 spawnPos, float spawnRot )
    {
        Vector3 position = isRandomPos ? GetRandomPosition() : new Vector3( spawnPos.x, 0, spawnPos.y );
        Quaternion rotation = isRandomPos ? GetRandomRotation() : Quaternion.Euler( 90, spawnRot, 0 );
        Ship thisShip = ( Ship ) Instantiate( Ship, position, rotation );
        thisShip.Team = team;
        thisShip.InvincibilityTimer = InvincibilityDuration;
        Instantiate( SpawnWarp, thisShip.transform.position, SpawnWarp.transform.rotation );
        return thisShip;
    }

    private Vector3 GetRandomPosition()
    {
        float x = UnityEngine.Random.Range( LevelXBounds.x, LevelXBounds.y );
        float z = UnityEngine.Random.Range( LevelZBounds.x, LevelZBounds.y );
        return new Vector3( x, 0, z );
    }

    private Quaternion GetRandomRotation()
    {
        return Quaternion.Euler( 90, UnityEngine.Random.Range( 0, 360 ), 0 );
    }

    public void ResetWeaponSpawnTimer()
    {
        weaponSpawnTimer = UnityEngine.Random.Range( WeaponSpawnRateRange.x, WeaponSpawnRateRange.y );
    }

    private TextMesh CreateRedText( Vector3 position )
    {
        TextMesh text = Instantiate( RedText, position, Quaternion.Euler( new Vector3( 90, 0, 0 ) ) );
        text.GetComponent<TextDestroy>().Duration = ShipSpawnDelay;
        return text;
    }

    private void UpdateScore()
    {
        PlayerScoreText.text = playerScore.ToString();
        OpponentScoreText.text = opponentScore.ToString();
    }

    private void UpdateHealthBars()
    {
        // Update player's health bar.
        float playerOffset = maxBarWidth;
        if( CurrentPlayer )
        {
            float playerBarWidth = ( float ) CurrentPlayer.Health / Ship.Health * maxBarWidth;
            playerOffset = maxBarWidth - playerBarWidth;
        }

        PlayerHealth.offsetMax = new Vector2( -playerOffset, PlayerHealth.offsetMax.y );

        // Update opponent's health bar.
        float opponentOffset = maxBarWidth;
        if( currentOpponent )
        {
            float opponentBarWidth = ( float ) currentOpponent.Health / Ship.Health * maxBarWidth;
            opponentOffset = maxBarWidth - opponentBarWidth;
        }

        OpponentHealth.offsetMin = new Vector2( opponentOffset, OpponentHealth.offsetMax.y );
    }

    private void UpdateWeaponText()
    {
        // Update player's weapon text.
        string playerWeapon = "";
        if( CurrentPlayer && CurrentPlayer.HeavyWeapon && 
            CurrentPlayer.HeavyWeapon.Ammo > 0 )
        {
            playerWeapon = CurrentPlayer.HeavyWeapon.Name + 
                " " + CurrentPlayer.HeavyWeapon.Ammo + "x";
        }

        PlayerWeaponText.text = playerWeapon;

        // Update opponent's weapon text.
        string opponentWeapon = "";
        if( currentOpponent && currentOpponent.HeavyWeapon && 
            currentOpponent.HeavyWeapon.Ammo > 0 )
        {
            opponentWeapon = currentOpponent.HeavyWeapon.Name +
                " " + currentOpponent.HeavyWeapon.Ammo + "x";
        }

        OpponentWeaponText.text = opponentWeapon;
    }

    private void UpdateTime()
    {
        gameTime -= Time.deltaTime;
        TimeSpan timeSpan = TimeSpan.FromSeconds( gameTime );
        string timeText = string.Format( "{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds );
        TimeText.text = timeText;

        // Time is up, so end the game.
        if( gameTime <= 0 )
        {
            gameTime = 0;
            currentState = GameState.Ended;
            endSound.Play();

            EndUI.enabled = true;
            string msg;
            // Player wins.
            if( playerScore > opponentScore )
            {
                msg = PLAYER_WIN;
                if( currentOpponent )
                    currentOpponent.Die();
            }
            // Opponent wins.
            else if( opponentScore > playerScore )
            {
                msg = OPPONENT_WIN;
                if( CurrentPlayer )
                    CurrentPlayer.Die();
            }
            // Nobody wins.
            else
            {
                msg = NO_WIN;
                if( currentOpponent )
                    currentOpponent.Die();
                if( CurrentPlayer )
                    CurrentPlayer.Die();
            }

            if( CurrentPickup )
                Destroy( CurrentPickup.gameObject );

            WinText.text = msg;
        }
    }

    // Register which ship picked up the weapon.
    public void UpdatePickupNGram( int team )
    {
        string value = ( team == 0 ) ? N_GRAM_PLAYER_ACTION : N_GRAM_OPPONENT_ACTION;

        pickupNGramSequence.Add( value );

        // If not enough actions in the sequence, then don't register it.
        if( pickupNGramSequence.Count == N_GRAM_WINDOW_SIZE + 1 )
        {
            pickupNGram.RegisterSequence( pickupNGramSequence );

            // Remove the oldest action.
            pickupNGramSequence.RemoveAt( 0 );
        }
    }
}
