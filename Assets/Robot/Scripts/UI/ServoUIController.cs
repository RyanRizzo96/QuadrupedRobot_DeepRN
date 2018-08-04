using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServoUIController : MonoBehaviour {

    public ServoMotor servo;

    private Robot robot; //added

    public Text title;
    public Text angleText;
    public Toggle motorToggle;
    public Toggle fixToggle;
    public InputField input;
    public Slider slider;

    private string prevValue;
    private float minAngle;
    private float maxAngle;

    private void Start()
    {
        if (!servo) return;

        minAngle = servo.isClockwise ? servo.minAngle : -servo.maxAngle;
        maxAngle = minAngle + servo.Range;

        title.text = title.text + " (Range: " + minAngle.ToString("0") + ".." + maxAngle.ToString("0") + "°)";
        input.text = servo.GetServoAngle().ToString();
        prevValue = input.text;
        motorToggle.onValueChanged.AddListener(OnMotorToggle);
        fixToggle.onValueChanged.AddListener(OnFixToggle);
        input.onValueChanged.AddListener(OnTextInputChanged);
        slider.minValue = minAngle;
        slider.maxValue = maxAngle;
        slider.value = servo.GetServoAngle();
        slider.onValueChanged.AddListener(OnSliderChanged);

        //servo.SetAngle(45);
        //robot.legs[0].lowerLeg.SetAngle(45);
    }

    private void Update()
    {
        if (!servo) return;

        //servo.GetServoAngle is current angle
        angleText.text = servo.GetServoAngle().ToString("0.0") + "°";
        motorToggle.isOn = servo.IsMotorEnabled;
        fixToggle.isOn = servo.IsFixed;
    }

    public void OnMotorToggle(bool value)
    {
        if (!servo) return;

        servo.IsMotorEnabled = motorToggle.isOn;
    }

    public void OnFixToggle(bool value)
    {
        if (!servo) return;

        servo.IsFixed = fixToggle.isOn;
    }

    public void OnTextInputChanged(string value)
    {
        if (!servo) return;

        //angle variable life cycle starts here
        float angle;

        //Converts the string representation of a number to its 32-bit signed integer equivalent.
        //A return value indicates whether the conversion succeeded.
        if (float.TryParse(value, out angle))
        {
            if (angle > maxAngle || angle < minAngle)
            {
                angle = Mathf.Clamp(angle, minAngle, maxAngle);
                input.text = angle.ToString();
            }
            servo.SetAngle(angle);
            prevValue = input.text;

            if (slider.value != angle)
            {
                slider.value = angle;
            }
        }
        else
        {
            input.text = prevValue;
        }
    }

    public void OnSliderChanged(float value)
    {
        servo.SetAngle(value);
        float textValue = 0;
        float.TryParse(input.text, out textValue);
        if (textValue != value)
        {
            input.text = value.ToString();
        }
    }
}
