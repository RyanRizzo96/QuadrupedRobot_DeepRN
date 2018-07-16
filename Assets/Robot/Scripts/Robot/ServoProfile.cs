using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Servo Profile", menuName = "Servo Profile", order = 0)]
public class ServoProfile : ScriptableObject
{
    [Header("Servo Settings")]
    [Range(0, 1)]
    public float delay = 0;
    [Range(0, 720)]
    public float maxVelocity = 180;
    [Range(0, 100)]
    public float maxForce = 10;

    [Header("Gizmo Settings")]
    [Range(0.1f, 5)]
    public float gizmoScale = 1;
    [Range(0.01f, 100)]
    public float gizmoMaxScaleDistance = 1;
}
