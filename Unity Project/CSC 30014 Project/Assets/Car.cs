using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    public AnimationCurve TorqueCurve;

    public List<float> Gears = new(); //Index 0 and 1 are reserverd for reverse and neutral, index 1 must = 0;
    public float WheelTorque;
    public float EngineTorque;
    public int CurrentGear;
    public float FinalDifferential;
    public float RPM;
    public float Throttle;

    // Start is called before the first frame update
    void Start()
    {
        //Set up engine
        SetTorqueCurve();
    }

    public void SetTorqueCurve()
    {
        TorqueCurve.AddKey(0, 0);
        TorqueCurve.AddKey(5500, 135);
        TorqueCurve.AddKey(7250, 0);
    }

    // Update is called once per frame
    void Update()
    {
        Throttle = Mathf.Clamp(Throttle, 0, 1);

        EngineTorque = Mathf.Clamp(TorqueCurve.Evaluate(RPM), 1000, 7250);

        WheelTorque = EngineTorque * Gears[CurrentGear] * FinalDifferential * Throttle;
    }
}
