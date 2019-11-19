using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public struct Weapons
{
    public WeaponTypes.Type Type;
    public Weapon Weapon;
}

public class WeaponTypes : MonoBehaviour
{
    public enum Type
    {
        Standard,
        Blaster,
        Spreader,
        Bomb,
        Nova,
        Vulcan,
        Cannon,
        Flak,
        PetalSpear
    }

    public List<Weapons> Weapons;

    public Weapon GetWeapon(WeaponTypes.Type type)
    {
        foreach (Weapons wpn in Weapons)
        {
            if (wpn.Type == type)
            {
                return wpn.Weapon;
            }
        }

        return null;
    }

    public Weapons GetRandomWeapon()
    {
        int index = UnityEngine.Random.Range(1, Weapons.Count);
        return Weapons[ index ];
    }
}
