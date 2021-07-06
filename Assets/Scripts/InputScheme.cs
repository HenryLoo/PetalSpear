using UnityEngine;

[CreateAssetMenu(fileName = "InputScheme", menuName = "InputScheme")]
public class InputScheme : ScriptableObject
{
    [SerializeField] private string rotate;
    [SerializeField] private string thrust;
    [SerializeField] private string standardWeapon;
    [SerializeField] private string heavyWeapon;
    [SerializeField] private string dodgeRoll;
    [SerializeField] private string cancel;

    public string Rotate { get { return rotate; } }
    public string Thrust { get { return thrust; } }
    public string StandardWeapon { get { return standardWeapon; } }
    public string HeavyWeapon { get { return heavyWeapon; } }
    public string DodgeRoll { get { return dodgeRoll; } }
    public string Cancel { get { return cancel; } }
}
