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

       
    }

    private void Update()
    {
        if (!robot) return;
        if (distanceText) distanceText.text = "Distance: " + robot.GetDistance().ToString("0.000") + "m";
        if (headingText) headingText.text = "Heading: " + robot.GetHeading().ToString("00.0") + "°";

 

        SetAngleCoroutine();
    }

    private IEnumerator SetAngleCoroutine()
    {
        //Yield return suspends routine execution for given amount if seconds using scaled time
        //If profile exisits use delay set there, else timescale set to 0 
        yield return new WaitForSeconds(profile ? profile.delay : 0);
        upperLeg1Servo.servo.SetAngle(Random.Range(-90.0f, 90.0f));
        yield return new WaitForSeconds(profile ? profile.delay : 0);
        upperLeg2Servo.servo.SetAngle(Random.Range(-90.0f, 90.0f));
        yield return new WaitForSeconds(profile ? profile.delay : 0);
        upperLeg3Servo.servo.SetAngle(Random.Range(-90.0f, 90.0f));
        yield return new WaitForSeconds(profile ? profile.delay : 0);
        upperLeg4Servo.servo.SetAngle(Random.Range(-90.0f, 90.0f));
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
