using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ServoMotor))]
public class JointFriction : MonoBehaviour
{
    public bool isDebugging = false;
    public FrictionProfile frictionProfile;
    private new Rigidbody rigidbody;
    private ServoMotor servo;

    private void Start()
    {
        servo = GetComponent<ServoMotor>();
        rigidbody = GetComponent<Rigidbody>();

        if (!servo || !rigidbody)
        {
            Debug.Log("Unable to apply friction to " + name + "! Components not found.");
        }
        if (!frictionProfile)
        {
            Debug.Log("Unable to apply friction to " + name + "! Friction profile not set.");
        }
    }

    /// <summary>
    /// Apply friction torque.
    /// </summary>
    private void FixedUpdate()
    {
        if (!servo || !rigidbody || !frictionProfile) return;

        //Vector 3 used to pass 3D positions and directions (x, y, z)
        //servo.axis is the local coordinates of the object, which is converted to global coordinates
        //multiplying direction of joint action by servo velocity and friction coefficients 
        Vector3 frictionTorque = -(transform.TransformDirection(servo.axis)).normalized * (servo.GetServoVelocity() * frictionProfile.viscousK + frictionProfile.staticK);
        rigidbody.AddTorque(frictionTorque);

        if (servo.servoBase)
        {
            servo.servoBase.AddTorque(-frictionTorque);
        }
        if (isDebugging)
        {
            Debug.DrawRay(transform.position, frictionTorque);
        }
    }
}
