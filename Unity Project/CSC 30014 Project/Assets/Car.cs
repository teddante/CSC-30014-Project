using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    public AnimationCurve TorqueCurve;

    public List<float> Gears = new();
    public float WheelTorque;
    public float EngineTorque;
    public int CurrentGear;
    public float FinalDifferential;
    public float RPM;
    public float Throttle;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Throttle = Mathf.Clamp(Throttle, 0, 1);

        EngineTorque = Mathf.Clamp((TorqueCurve.Evaluate(RPM) * Throttle), 1000, 7250);

        WheelTorque = EngineTorque * Gears[CurrentGear] * FinalDifferential;
    }
}
