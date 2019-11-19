using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public Vector2 LevelXBounds;
    public Vector2 LevelZBounds;

    public Vector2 PlayerSpawnPos;
    public float PlayerSpawnRot = 0;
    public Vector2 OpponentSpawnPos;
    public float OpponentSpawnRot = 180;

    public float ShipSpawnDelay;
    private float playerSpawnTimer;
    private float opponentSpawnTimer;
    private bool isReadyToSpawnPlayer = true;
    private bool isReadyToSpawnOpponent = true;
    public GameObject SpawnWarp;

    public WeaponPickup WeaponPickup;
    private WeaponTypes weaponTypes;
    public Vector2 WeaponSpawnRateRange;
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
    private bool hasGameStarted = false;

    public float InvincibilityDuration;

    void Awake()
    {
        weaponTypes = GetComponent<WeaponTypes>();
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Title screen.
        if( !hasGameStarted )
        {
            float fire = Input.GetAxis( "Fire1" );
            if( fire != 0 )
                StartGame();
        }
        // Game logic.
        else
        {
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
                    isReadyToSpawnWeapon = true;
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
    }

    private void StartGame()
    {
        // Hide title screen.
        if( TitleUI.enabled )
            TitleUI.enabled = false;

        SpawnPlayer( false );
        SpawnOpponent( false );
        ResetWeaponSpawnTimer();
        hasGameStarted = true;
    }

    private void SpawnWeapon()
    {
        // A weapon already exists, so don't spawn another one.
        if( CurrentPickup )
            return;

        Weapons wpn = weaponTypes.GetRandomWeapon();
        Vector3 position = GetRandomPosition();
        WeaponPickup pickup = ( WeaponPickup ) Instantiate( WeaponPickup, position, transform.rotation );
        pickup.WeaponType = wpn.Type;
        pickup.GetComponent<Renderer>().material.SetColor( "_Color", wpn.Weapon.Colour );
        CurrentPickup = pickup;
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
        float x = Random.Range( LevelXBounds.x, LevelXBounds.y );
        float z = Random.Range( LevelZBounds.x, LevelZBounds.y );
        return new Vector3( x, 0, z );
    }

    private Quaternion GetRandomRotation()
    {
        return Quaternion.Euler( 90, Random.Range( 0, 360 ), 0 );
    }

    public void ResetWeaponSpawnTimer()
    {
        weaponSpawnTimer = Random.Range( WeaponSpawnRateRange.x, WeaponSpawnRateRange.y );
    }

    private TextMesh CreateRedText( Vector3 position )
    {
        TextMesh text = Instantiate( RedText, position, Quaternion.Euler( new Vector3( 90, 0, 0 ) ) );
        text.GetComponent<TextDestroy>().Duration = ShipSpawnDelay;
        return text;
    }
}
