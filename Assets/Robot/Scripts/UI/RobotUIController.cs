using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RobotUIController : MonoBehaviour
{
    public float current_angle;

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
    public ServoUIController upperLeg1Servo;
    [SerializeField]
    public ServoUIController lowerLeg1Servo;
    [SerializeField]
    public ServoUIController upperLeg2Servo;
    [SerializeField]
    public ServoUIController lowerLeg2Servo;
    [SerializeField]
    public ServoUIController upperLeg3Servo;
    [SerializeField]
    public ServoUIController lowerLeg3Servo;
    [SerializeField]
    public ServoUIController upperLeg4Servo;
    [SerializeField]
    public ServoUIController lowerLeg4Servo;
    [SerializeField]
    private ServoProfile profile;

    private void Awake()
    {
        upperLeg1Servo.servo = robot.legs[0].upperLeg;
        lowerLeg1Servo.servo = robot.legs[0].lowerLeg;
        upperLeg2Servo.servo = robot.legs[1].upperLeg;
        lowerLeg2Servo.servo = robot.legs[1].lowerLeg;
        upperLeg3Servo.servo = robot.legs[2].upperLeg;
        lowerLeg3Servo.servo = robot.legs[2].lowerLeg;
        upperLeg4Servo.servo = robot.legs[3].upperLeg;
        lowerLeg4Servo.servo = robot.legs[3].lowerLeg;

        toggleUpper.onValueChanged.AddListener(ToggleUpperLegs);
        toggleLower.onValueChanged.AddListener(ToggleLowerLegs);

        for (int i = 0; i < 4; i++)
        {
           // robot.legs[i].upperLeg.SetAngle(Random.Range(-90.0f, 90.0f));
        }
        
    }

    private void Update()
    {
        if (!robot) return;
        if (distanceText) distanceText.text = "Distance: " + robot.GetDistance().ToString("0.000") + "m";
        if (headingText) headingText.text = "Heading: " + robot.GetHeading().ToString("00.0") + "°";

        StartCoroutine(SetAngleCoroutine());
    }

    private IEnumerator SetAngleCoroutine()
    {
        //Yield return suspends routine execution for given amount if seconds using scaled time
        //If profile exisits use delay set there, else timescale set to 0 

        for (int i = 0; i < 4; i++)
        {
            float target = Random.Range(-90.0f, 90.0f);
            robot.legs[i].upperLeg.SetAngle(target);

            if (current_angle == target)
            {

            }

            else
            {
                yield return new WaitForSeconds(1);
            }
           
        }

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
