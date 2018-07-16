using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RobotUIController : MonoBehaviour
{
    [SerializeField]
    private Robot robot;
    [SerializeField]
    private Toggle toggleUpper;
    [SerializeField]
    private Toggle toggleLower;
    [SerializeField]
    private Text distanceText;
    [SerializeField]
    private Text headingText;
    [SerializeField]
    private ServoUIController upperLeg1Servo;
    [SerializeField]
    private ServoUIController lowerLeg1Servo;
    [SerializeField]
    private ServoUIController upperLeg2Servo;
    [SerializeField]
    private ServoUIController lowerLeg2Servo;
    [SerializeField]
    private ServoUIController upperLeg3Servo;
    [SerializeField]
    private ServoUIController lowerLeg3Servo;
    [SerializeField]
    private ServoUIController upperLeg4Servo;
    [SerializeField]
    private ServoUIController lowerLeg4Servo;

    private void Awake()
    {
        upperLeg1Servo.servo = robot.legs[0].upperLeg;
        lowerLeg1Servo.servo = robot.legs[0].lowerLeg ;
        upperLeg2Servo.servo = robot.legs[1].upperLeg;
        lowerLeg2Servo.servo = robot.legs[1].lowerLeg;
        upperLeg3Servo.servo = robot.legs[2].upperLeg;
        lowerLeg3Servo.servo = robot.legs[2].lowerLeg;
        upperLeg4Servo.servo = robot.legs[3].upperLeg;
        lowerLeg4Servo.servo = robot.legs[3].lowerLeg;

        toggleUpper.onValueChanged.AddListener(ToggleUpperLegs);
        toggleLower.onValueChanged.AddListener(ToggleLowerLegs);
    }

    private void Update()
    {
        if (!robot) return;
        if (distanceText) distanceText.text = "Distance: " + robot.GetDistance().ToString("0.000") + "m";
        if (headingText) headingText.text = "Heading: " + robot.GetHeading().ToString("00.0") + "°";
    }

    public void ToggleUpperLegs(bool value)
    {
        foreach (var leg in robot.legs)
        {
            leg.upperLeg.IsFixed = value;
        }
    }

    public void ToggleLowerLegs(bool value)
    {
        foreach (var leg in robot.legs)
        {
            leg.lowerLeg.IsFixed = value;
        }

    }
}
