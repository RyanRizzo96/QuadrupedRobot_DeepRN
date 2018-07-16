using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PIDRegulator
{
    private PIDProfile profile;

    private float integratorValue = 0;
    private float prevError = 0;

    public PIDRegulator (PIDProfile p)
    {
        profile = p;
    }

    //Run(ServoAngleToJointSpace(targetAngle), GetJointAngle(), Time.fixedDeltaTime);
    public float Run(float target, float current, float deltaTime)
    {
        if (!profile) return 0;

        float error = target - current;
        integratorValue = integratorValue + error * deltaTime;
        integratorValue = Mathf.Clamp(integratorValue, -profile.iLimit, profile.iLimit);
        float res = profile.p * error
                    + profile.i * integratorValue
                    + profile.d * (error -  prevError) / deltaTime;

        prevError = error;

        //result returned to Servo.cs (targetVelocity)
        return res;
    }
}
