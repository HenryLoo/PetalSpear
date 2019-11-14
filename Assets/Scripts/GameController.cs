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

    public GameObject RedText;
    private const string DESTROYED_TEXT = "DESTROYED";
    private TextMesh playerDestroyed;
    private TextMesh opponentDestroyed;
    private Vector3 playerPos;
    private Vector3 opponentPos;

    void Awake()
    {
        weaponTypes = GetComponent<WeaponTypes>();
    }

    // Use this for initialization
    void Start()
    {
        SpawnPlayer( false );
        SpawnOpponent( false );
        ResetWeaponSpawnTimer();
    }

    // Update is called once per frame
    void Update()
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
                Destroy( playerDestroyed );
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
                Destroy( opponentDestroyed );
            }
        }
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

        Vector3 position = isRandomPos ? GetRandomPosition() : new Vector3( PlayerSpawnPos.x, 0, PlayerSpawnPos.y );
        Quaternion rotation = isRandomPos ? GetRandomRotation() : Quaternion.Euler( 90, PlayerSpawnRot, 0 );
        CurrentPlayer = ( Ship ) Instantiate( Ship, position, rotation );
        Player player = ( Player ) Instantiate( Player, position, CurrentPlayer.transform.rotation );
        player.transform.SetParent( CurrentPlayer.transform );
        player.Ship = CurrentPlayer;
        CurrentPlayer.Team = 0;
        GameCamera.Target = CurrentPlayer.transform;
    }

    private void SpawnOpponent( bool isRandomPos )
    {
        // Opponent already exists, so don't spawn another one.
        if( currentOpponent )
            return;

        Vector3 position = isRandomPos ? GetRandomPosition() : new Vector3( OpponentSpawnPos.x, 0, OpponentSpawnPos.y );
        Quaternion rotation = isRandomPos ? GetRandomRotation() : Quaternion.Euler( 90, OpponentSpawnRot, 0 );
        currentOpponent = ( Ship ) Instantiate( Ship, position, rotation );
        AIEngine opponent = ( AIEngine ) Instantiate( AIEngine, position, currentOpponent.transform.rotation );
        opponent.transform.SetParent( currentOpponent.transform );
        opponent.Game = this;
        currentOpponent.Team = 1;
        opponent.Ship = currentOpponent;
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
        GameObject obj = Instantiate( RedText, position, Quaternion.Euler( new Vector3( 90, 0, 0 ) ) );
        TextMesh text = obj.GetComponent<TextMesh>();
        return text;
    }
}
