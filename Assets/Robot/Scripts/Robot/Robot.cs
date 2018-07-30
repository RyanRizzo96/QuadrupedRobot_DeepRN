using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Robot : MonoBehaviour
{
    private Robot robot;

    public float forwardDirectionOffset = 0;

    public Leg[] legs = new Leg[4];

    private Vector3 initialDirection;
    private Vector3 initialPosition;

    /// <summary>
    /// Store initial position and rotation.
    /// </summary>
    private void Awake()
    {
        initialDirection = GetForwardDirection();
        initialPosition = transform.position;
    }

    public void Update()
    {

        //robot.legs[1].lowerLeg.SetAngle(45);
    }

    /// <summary>
    /// Get look direction of the robot.
    /// </summary>
    private Vector3 GetForwardDirection()
    {
        return Vector3.ProjectOnPlane(Quaternion.AngleAxis(forwardDirectionOffset, transform.up) * transform.forward, Vector3.up);
    }

    /// <summary>
    /// Get current robot heading in degrees.
    /// </summary>
    public float GetHeading()
    {
        return Vector3.SignedAngle(initialDirection, GetForwardDirection(), Vector3.up);
    }

    /// <summary>
    /// Get distance walked forward.
    /// </summary>
    public float GetDistance()
    {
        Vector3 movementVector = Vector3.Project((transform.position - initialPosition), initialDirection);
        return movementVector.magnitude * (Vector3.Angle(movementVector, initialDirection) > 90 ? -1 : 1);
    }

    /// <summary>
    /// Fixes positions of all legs.
    /// </summary>
    public void FixLegs()
    {
        FixUpperLegs();
        FixLowerLegs();
    }

    /// <summary>
    /// Unfixes positions of all legs.
    /// </summary>
    public void UnfixLegs()
    {
        UnfixUpperLegs();
        UnfixLowerLegs();
    }

    /// <summary>
    /// Fixes positions of all four upper legs.
    /// </summary>
    public void FixUpperLegs()
    {
        foreach (var leg in legs)
            if (leg != null && leg.upperLeg != null)
                leg.upperLeg.IsFixed = true;
    }

    /// <summary>
    /// Unfixes positions of all four upper legs.
    /// </summary>
    public void UnfixUpperLegs()
    {
        foreach (var leg in legs)
            if (leg != null && leg.upperLeg != null)
                leg.upperLeg.IsFixed = false;
    }

    /// <summary>
    /// Fixes positions of all four lower legs.
    /// </summary>
    public void FixLowerLegs()
    {
        foreach (var leg in legs)
            if (leg != null && leg.upperLeg != null)
                leg.lowerLeg.IsFixed = true;
    }

    /// <summary>
    /// Unfixes positions of all four lower legs.
    /// </summary>
    public void UnfixLowerLegs()
    {
        foreach (var leg in legs)
            if (leg != null && leg.upperLeg != null)
                leg.lowerLeg.IsFixed = false;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Draw debug info.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            initialPosition = transform.position;
            initialDirection = GetForwardDirection();
        }

        // Draw walked distance
        Gizmos.color = Color.black;
        Gizmos.DrawLine(initialPosition, initialPosition + initialDirection * GetDistance());
    }

    /// <summary>
    /// Draw debug info.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            initialPosition = transform.position;
            initialDirection = GetForwardDirection();
        }

        // Draw walking direction
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.ArrowHandleCap(-1,
                                            transform.position,
                                            Quaternion.FromToRotation(Vector3.forward, initialDirection),
                                            UnityEditor.HandleUtility.GetHandleSize(transform.position),
                                            EventType.Repaint);
    }
#endif
}

[System.Serializable]
public class Leg
{
    public ServoMotor upperLeg;
    public ServoMotor lowerLeg;
}

