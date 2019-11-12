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

    public WeaponPickup WeaponPickup;
    private WeaponTypes weaponTypes;
    public Vector2 WeaponSpawnRateRange;
    private float weaponSpawnTimer;

    public CameraFollow GameCamera;
    public Ship Ship;
    public Player Player;
    public AIEngine AIEngine;

    private Ship currentPlayer;
    private Ship currentOpponent;
    public WeaponPickup CurrentPickup;

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
        if( weaponSpawnTimer > 0 )
        {
            weaponSpawnTimer -= Time.deltaTime;
            weaponSpawnTimer = Mathf.Max( weaponSpawnTimer, 0 );
        }

        if( weaponSpawnTimer == 0 && !CurrentPickup )
        {
            // TODO: support more weapon types.
            SpawnWeapon();
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
        pickup.Game = this;
        CurrentPickup = pickup;
    }

    private void SpawnPlayer( bool isRandomPos )
    {
        // Player already exists, so don't spawn another one.
        if( currentPlayer )
            return;

        Vector3 position = isRandomPos ? GetRandomPosition() : new Vector3( PlayerSpawnPos.x, 0, PlayerSpawnPos.y );
        Quaternion rotation = isRandomPos ? GetRandomRotation() : Quaternion.Euler( 90, PlayerSpawnRot, 0 );
        currentPlayer = ( Ship ) Instantiate( Ship, position, rotation );
        Player player = ( Player ) Instantiate( Player, position, currentPlayer.transform.rotation );
        player.transform.SetParent( currentPlayer.transform );
        player.Ship = currentPlayer;
        currentPlayer.Team = 0;
        GameCamera.Target = currentPlayer.transform;
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
        opponent.PlayerShip = currentPlayer;
        currentPlayer.Team = 1;
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
}
