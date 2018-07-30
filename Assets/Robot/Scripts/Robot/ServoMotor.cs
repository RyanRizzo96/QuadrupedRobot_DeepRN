using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Rigidbody))]
public class ServoMotor : MonoBehaviour
{
    public bool isClockwise = true;

    private Robot robot; //added

    [Space]
    [SerializeField]
    private float targetAngle = 0;

    [Header("Geometry")]    //Used to add a header above fields in inspector
    public Rigidbody servoBase; //this is robot body
    public Vector3 anchor = Vector3.zero;
    public Vector3 axis = Vector3.right;
    public float minAngle = -180;
    public float maxAngle = 180;
    public float angleGizmoOffset = 0;

    // Joint limits should be set a little wider than servo limits
    // Otherwise unexpected behaviour on the boundaries occures
    private const float jointLimitsOffset = 1f;

    [Header("Physics")]
    [SerializeField]
    private ServoProfile profile;

    [Header("Feedback")]
    [SerializeField]
    private PIDProfile positionRegulatorProfile;
    [SerializeField]
    private PIDProfile velocityRegulatorProfile;

    private PIDRegulator positionRegulator;
    private PIDRegulator velocityRegulator;

    private HingeJoint joint;
    private new Rigidbody rigidbody;
    private Vector3 correctedAxis;
    private Vector3 axisRelativeToBase;
    private Vector3 zeroDirection;
    
    public float Range
    {
        get
        {
            return maxAngle - minAngle;
        }
    }

    /// <summary>
    /// Fixing servo is similar to replacing HingeJoint with a FixedJoint.
    /// </summary>
    private bool _isFixed;
    public bool IsFixed
    {
        get
        {
            return _isFixed;
        }
        set
        {
            if (_isFixed == value || joint == null) return;
            _isFixed = value;

            // Fix servo position
            if (_isFixed)
            {
                targetAngle = GetServoAngle();
                var jointAngle = GetJointAngle();
                joint.limits = new JointLimits() { bounceMinVelocity = float.MaxValue, min = jointAngle - 0.001f, max = jointAngle };
            }
            // Unfix servo position
            else
            {
                joint.limits = new JointLimits() { bounceMinVelocity = float.MaxValue, min = minAngle - jointLimitsOffset, max = maxAngle + jointLimitsOffset };
            }
        }
    }

    /// <summary>
    /// An interface to access joint.useMotor. The value is cached to increase performance,
    /// because accessing joint.useMotor normally takes some processing time.
    /// </summary>
    private bool _isMotorEnabled;
    public bool IsMotorEnabled
    {
        get
        {
            return _isMotorEnabled;
        }
        set
        {
            if (_isMotorEnabled == value || joint == null) return;
            joint.useMotor = _isMotorEnabled = value;
        }
    }

    /// <summary>
    /// Initialize servo.
    /// </summary>
    private void Awake()
    {
        if (Application.isPlaying)
        {
            correctedAxis = GetCorrectedAxis();
            rigidbody = GetComponent<Rigidbody>();
            positionRegulator = new PIDRegulator(positionRegulatorProfile);
            velocityRegulator = new PIDRegulator(velocityRegulatorProfile);

            CreateJoint();

            if (!profile || !positionRegulatorProfile)
            {
                Debug.Log("Servo " + name + " will not work properly, because it is not fully configured!");
            }
        }
    }

    /// <summary>
    /// Creates HingeJoint.
    /// </summary>
    private void CreateJoint()
    {
        //gameObject is a Base class for all entities in Unity scenes, Adds a component class named className to the game object.
        joint = gameObject.AddComponent<HingeJoint>();
        joint.connectedBody = servoBase;
        joint.axis = correctedAxis;
        joint.anchor = anchor;
        joint.useLimits = true;
        joint.limits = new JointLimits() { bounceMinVelocity = float.PositiveInfinity, min = minAngle - jointLimitsOffset, max = maxAngle + jointLimitsOffset };

        IsMotorEnabled = true;
        IsFixed = false;
        axisRelativeToBase = GetAxisRelativeToBase();
        zeroDirection = GetJointDirection();
        targetAngle = JointAngleToServoSpace(GetJointAngle());
    }

    /// <summary>
    /// If axis vector equals to 0, 0, 0, by default Unity uses X axis.
    /// </summary>
    private Vector3 GetCorrectedAxis()
    {
        //right means (1,0,0)
        return axis.magnitude == 0 ? Vector3.right : axis.normalized;
    }

    /// <summary>
    /// Calculates axis direction relative to the base object.
    /// </summary>
    /// <returns></returns>
    private Vector3 GetAxisRelativeToBase()
    {
        //Transform represents Position, rotation and scale of an object. The rotation of the transform in world space stored as a Quaternion.
        Vector3 res = transform.rotation * correctedAxis;
        if (servoBase)
        {
            //Quaternions are used to represent rotations.
            res = Quaternion.Inverse(servoBase.transform.rotation) * res;
        }
        return res;
    }

    /// <summary>
    /// Execute every frame.
    /// </summary>
    private void Update()
    {
        // In editor mode, joint look direction is always zero direction
        // Returns true in the Unity editor when in play mode.
        if (!Application.isPlaying)
        {
            correctedAxis = GetCorrectedAxis();
            zeroDirection = GetJointDirection();
            axisRelativeToBase = GetAxisRelativeToBase();
        }
    }

    /// <summary>
    /// Servo motor physics.
    /// </summary>
    private void FixedUpdate()
    {
        if (!IsMotorEnabled || IsFixed || !positionRegulatorProfile || !profile) return;

        // Run servo motor
        if (IsMotorEnabled)
        {
            // Angle regulator controls servo velocity based on the position error
            var targetVelocity = positionRegulator.Run(ServoAngleToJointSpace(targetAngle), GetJointAngle(), Time.fixedDeltaTime);

            //Clamps target velocity according to servo profile
            targetVelocity = Mathf.Clamp(targetVelocity, -profile.maxVelocity, profile.maxVelocity);

            // Unity is quite bad at keeping target velocity, so we might use an extra regulator, which 
            // takes velocity error as an input. Even though we write its output value to joint.motor.targetVelocity,
            // it works more like a torque control.

            //Basically this line calculates the velocity error. Will add later on.
            var velocityCorrection = velocityRegulator.Run(targetVelocity, GetServoVelocity(), Time.fixedDeltaTime);
            velocityCorrection = Mathf.Clamp(velocityCorrection, -500, 500);
            //joint motor applies maxForce Available and sets target vel
            joint.motor = new JointMotor() { force = profile.maxForce, freeSpin = false, targetVelocity = targetVelocity + velocityCorrection };
        }
    }

    /// <summary>
    /// Get angular velocity in degrees per second.
    /// </summary>
    public float GetServoVelocity()
    {
        // We have to calculate servo velocity manually, because HingeJoint.velocity shows target velocity 
        // and not the actual velocity of the rigidbody
        Vector3 velocity = rigidbody.angularVelocity * Mathf.Rad2Deg;
        
        //we want to get servo velocity thats why we subtract from robot body.
        if (servoBase) velocity = velocity - servoBase.angularVelocity;
        //transforms vector from world to local space
        velocity = transform.InverseTransformVector(velocity);

        //returns dot product of 2 vectors. Angular velocity is angular displacement relative to origin
        return Vector3.Dot(velocity, correctedAxis);
    }

    /// <summary>
    /// Get angle.
    /// </summary>
    public float GetServoAngle()
    {
        //The output of this function is determined by clockwise or anticlockwise servo rotation
        return JointAngleToServoSpace(GetJointAngle());
    }

    /// <summary>
    /// Get joint angle in degrees.
    /// </summary>
    private float GetJointAngle()
    {
        // We have to calculate joint angle manually, because HingeJoint.angle is broken since Unity 5.2 
        //Returns the signed angle in degrees between from and to, about axis.
        //The smaller of the two possible angles between the two vectors is returned, therefore the result will never be greater than 180 degrees or smaller than -180 degrees.
        return Vector3.SignedAngle(zeroDirection, GetJointDirection(), axisRelativeToBase);
    }

    /// <summary>
    /// Look direction of the joint. Used in the angle calculations by Unity.
    /// </summary>
    private Vector3 GetJointDirection()
    {
        // Direction from the unscaled joint center to the transform position
        Vector3 dir = transform.position - GetUnscaledJointPosition();

        // Or just use one of the local axis' if joint center is located at the transform position (anchor = 0, 0, 0)
        // or dir vector is perpendicular to the rotation plane
        // The length (magnitude) of the vector is square root of (x*x+y*y+z*z).
        if (dir.magnitude == 0 || Mathf.Abs(Vector3.Dot(dir.normalized, transform.rotation * correctedAxis)) == 1)
        {
            // If angle between Z axis and the joint axis is more or equals to 45 degrees, use Z axis
            if (Mathf.Abs(Vector3.Dot(Vector3.forward, correctedAxis)) <= Mathf.Cos(45 * Mathf.Deg2Rad))
                dir = transform.forward;
            // Otherwise use X axis (don't ask me why, that's how Unity PhysX does it)
            else
                //Transform.right moves the GameObject in the red arrow’s axis (X).
                dir = transform.right;
        }
        
        // Calculated relative to the base object
        if (servoBase)
        {
            //.inverse returns the inverse of the rotation
            //To get the new position after rotation, multiply both vectors.
            dir = Quaternion.Inverse(servoBase.transform.rotation) * dir;
        }

        // And projected onto the rotation plane
        dir = Vector3.ProjectOnPlane(dir, axisRelativeToBase);

        //When normalized, a vector keeps the same direction but its length is 1.0.
        return dir.normalized;
    }

    /// <summary>
    /// Unscaled joint position is used in the angle calcultions by Unity.
    /// It is the same as the actual joint center for Transform.localScale = 1, 1, 1
    /// </summary>
    private Vector3 GetUnscaledJointPosition()
    {
        //Position of the Transform in X, Y, and Z coordinates,
        //Rotation of the Transform around the X, Y, and Z axes, measured in degrees,
        return transform.position + transform.rotation * anchor;
    }

    /// <summary>
    /// Set target angle.
    /// </summary>
    public void SetAngle(float angle)
    {
        StartCoroutine(SetAngleCoroutine(angle));
    }

    /// <summary>
    /// Simulate servo delay.
    /// </summary>
    private IEnumerator SetAngleCoroutine(float angle)
    {
        //Yield return suspends routine execution for given amount if seconds using scaled time
        //If profile exisits use delay set there, else timescale set to 0 
        yield return new WaitForSeconds(profile ? profile.delay : 0);
        //clamps angle value between min angle and max angle, depending if turning clockwise or anticlockwise
        targetAngle = Mathf.Clamp(angle, isClockwise ? minAngle : -maxAngle , isClockwise ? maxAngle : -minAngle);
    }

    /// <summary>
    /// Convert servo input angle to joint space (inverse if counter clockwise)
    /// </summary>
    private float ServoAngleToJointSpace(float a)
    {
        if (!isClockwise) a = -a;
        return Mathf.Clamp(a, minAngle, maxAngle);
    }

    /// <summary>
    /// Convert joint angle to servo input/output space (inverse if counter clockwise)
    /// </summary>
    private float JointAngleToServoSpace(float a)
    {
        if (!isClockwise) a = -a;
        return a;
    }

#if UNITY_EDITOR

    /// <summary>
    /// Visualize servo operation.
    /// </summary>
    private void OnDrawGizmos()
    {
        Vector3 globalAnchor = transform.TransformPoint(anchor);
        Vector3 globalAxis = transform.TransformDirection(correctedAxis);
        float servoAngle = GetServoAngle();
        float gizmoScale = Mathf.Min(UnityEditor.HandleUtility.GetHandleSize(globalAnchor), 
            profile ? profile.gizmoMaxScaleDistance : 1) * (profile ? profile.gizmoScale : 1);

        // Draw joint anchor and rotation axis
        UnityEditor.Handles.color = targetAngle > servoAngle + 0.1f ? Color.red : targetAngle < servoAngle - 0.1f ? Color.blue : Color.green;
        UnityEditor.Handles.CylinderHandleCap(1, globalAnchor, Quaternion.FromToRotation(Vector3.forward, globalAxis), 0.4f * gizmoScale, EventType.Repaint);
    }

    /// <summary>
    /// Visualize servo operation when servo selected.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Quaternion baseRotation = servoBase ? servoBase.rotation : Quaternion.identity;
        Vector3 globalAnchor = transform.TransformPoint(anchor);
        Vector3 globalAxis = transform.TransformDirection(correctedAxis);
        Vector3 globalZeroDirection = baseRotation * zeroDirection;

        if (!Application.isPlaying) targetAngle = GetServoAngle();

        float gizmoScale = Mathf.Min(UnityEditor.HandleUtility.GetHandleSize(globalAnchor), 
            profile ? profile.gizmoMaxScaleDistance : 1) * (profile ? profile.gizmoScale : 1);

        // Draw joint limits
        UnityEditor.Handles.color = IsMotorEnabled && !IsFixed ? new Color(0, 0, 1f, 0.2f) : new Color(0f, 0f, 0f, 0.2f);
        UnityEditor.Handles.DrawSolidArc(globalAnchor, globalAxis, Quaternion.AngleAxis(minAngle, globalAxis) * 
            (Quaternion.AngleAxis(angleGizmoOffset, globalAxis) * globalZeroDirection), maxAngle - minAngle, 1.2f * gizmoScale);
        // Draw direction to the target position of the joint
        UnityEditor.Handles.color = Color.yellow;
        UnityEditor.Handles.DrawLine(globalAnchor, globalAnchor + Quaternion.AngleAxis(ServoAngleToJointSpace(targetAngle), globalAxis) * 
            (Quaternion.AngleAxis(angleGizmoOffset, globalAxis) * globalZeroDirection) * 1.2f * gizmoScale);
        // Draw direction to the current position of the joint
        UnityEditor.Handles.color = Color.green;
        UnityEditor.Handles.DrawLine(globalAnchor, globalAnchor + baseRotation * 
            (Quaternion.AngleAxis(angleGizmoOffset, globalAxis) * GetJointDirection()) * 1.2f * gizmoScale);
    }
#endif


    //robot.legs[0].upperLeg.setAngle(45);
}

public class RandomMovement
{

    private Robot robot;

    //Testing a basic loop which chooses random values for joint target angles
    public void randomAngle()
    {
        for (int i = 0; i < 3; i++)
        {
            //robot.legs[i].lowerLeg.SetAngle(Random.Range(-45.0f, 45.0f));
            robot.legs[i].lowerLeg.SetAngle(45);
        }

        for (int i = 0; i < 3; i++)
        {
            //robot.legs[i].upperLeg.SetAngle(Random.Range(-45.0f, 45.0f));
            robot.legs[i].lowerLeg.SetAngle(45);
        }

    }
}