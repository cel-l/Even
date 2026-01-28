using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
namespace Even.Interaction;

public class InputAction
{
    private bool lastState;
    private float lastPressedTime;

    private readonly System.Func<bool> boolGetter;
    private readonly System.Func<float> floatGetter;
    private readonly float floatThreshold;

    public InputAction(System.Func<bool> boolGetter)
    {
        this.boolGetter = boolGetter;
    }

    public InputAction(System.Func<float> floatGetter, float threshold = 0.8f)
    {
        this.floatGetter = floatGetter;
        this.floatThreshold = threshold;
    }

    public void Update()
    {
        bool current = boolGetter?.Invoke() ?? (floatGetter() > floatThreshold);

        if (current && !lastState)
            lastPressedTime = Time.time;

        lastState = current;
    }

    public bool IsPressing() => lastState;

    public bool WasPressed() => lastState && Mathf.Approximately(lastPressedTime, Time.time);

    public float TimeSincePressed() => Time.time - lastPressedTime;
}

public class Input : MonoBehaviour
{
    public InputAction LeftPrimary;
    public InputAction RightPrimary;

    public InputAction LeftSecondary;
    public InputAction RightSecondary;

    public InputAction LeftTrigger;
    public InputAction RightTrigger;

    public InputAction LeftGrip;
    public InputAction RightGrip;

    public InputAction LeftJoystickClick;
    public InputAction RightJoystickClick;

    private void Awake()
    {
        LeftPrimary   = new InputAction(() => ControllerInputPoller.instance.leftControllerPrimaryButton);
        RightPrimary  = new InputAction(() => ControllerInputPoller.instance.rightControllerPrimaryButton);

        LeftSecondary  = new InputAction(() => ControllerInputPoller.instance.leftControllerSecondaryButton);
        RightSecondary = new InputAction(() => ControllerInputPoller.instance.rightControllerSecondaryButton);

        LeftTrigger  = new InputAction(() => ControllerInputPoller.instance.leftControllerIndexFloat);
        RightTrigger = new InputAction(() => ControllerInputPoller.instance.rightControllerIndexFloat);

        LeftGrip  = new InputAction(() => ControllerInputPoller.instance.leftControllerGripFloat);
        RightGrip = new InputAction(() => ControllerInputPoller.instance.rightControllerGripFloat);

        LeftJoystickClick  = new InputAction(() => GetJoystickClick(XRNode.LeftHand));
        RightJoystickClick = new InputAction(() => GetJoystickClick(XRNode.RightHand));
    }

    private void Update()
    {
        LeftPrimary.Update();
        RightPrimary.Update();

        LeftSecondary.Update();
        RightSecondary.Update();

        LeftTrigger.Update();
        RightTrigger.Update();

        LeftGrip.Update();
        RightGrip.Update();

        LeftJoystickClick.Update();
        RightJoystickClick.Update();
    }

    private bool GetJoystickClick(XRNode node)
    {
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(node, devices);

        foreach (var d in devices)
        {
            if (d.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool click))
                return click;

            if (d.TryGetFeatureValue(CommonUsages.secondary2DAxisClick, out bool secClick))
                return secClick;
        }

        return false;
    }

    public Vector2 LeftJoystickAxis => ControllerInputPoller.instance.leftControllerPrimary2DAxis;
    public Vector2 RightJoystickAxis => ControllerInputPoller.instance.rightControllerPrimary2DAxis;
}