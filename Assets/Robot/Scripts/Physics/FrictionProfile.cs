using UnityEngine;

/// <summary>
/// Global Coulomb and viscous friction settings.
/// </summary>
[CreateAssetMenu(fileName = "New Friction Profile", menuName = "Friction Profile", order = 0)]
public class FrictionProfile : ScriptableObject
{
    public float viscousK;
    public float staticK;
}
