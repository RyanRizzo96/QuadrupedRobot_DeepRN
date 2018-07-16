using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New PID Regulator Profile", menuName = "PID Regulator Profile", order = 0)]
public class PIDProfile : ScriptableObject
{
    public float p = 10;
    public float i = 0;
    public float d = 0;
    public float iLimit = 0;
}
