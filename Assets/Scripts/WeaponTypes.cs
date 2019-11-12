using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public struct Weapons
{
    public WeaponTypes.Type Type;
    public string Name;
    public Weapon Weapon;
}

public class WeaponTypes : MonoBehaviour
{
    public enum Type
    {
        Standard
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
}
