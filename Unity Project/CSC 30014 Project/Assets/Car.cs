using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEditor;
using UnityEngine;

public class Car : Agent
{
    public AnimationCurve TorqueCurve;

    //Inputs
    public float Steering;
    public float Throttle;
    public float Brake;

    //Other
    public List<float> Gears = new(); //Index 0 and 1 are reserverd for reverse and neutral, index 1 must = 0;
    public float WheelTorque;
    public float EngineTorque;
    public int CurrentGear;
    public float FinalDifferential;
    public float RPM;
    public WheelCollider RL;
    public WheelCollider RR;
    public float Speed;
    public float ZTSTimer;
    public float FrontBrakePeakNM;
    public float RearBrakePeakNM;
    
    private Rigidbody rBody;

    // Start is called before the first frame update
    void Start()
    {
        rBody = GetComponent<Rigidbody>();

        //Set up engine
        SetTorqueCurve();
        SetupGearBox();

        //Set Brakes
        //1400nm is the peak Brake force for the Mazda
        FrontBrakePeakNM = 1400 * 0.67f;
        RearBrakePeakNM = 1400 * (1 - 0.67f);
    }

    public void SetupGearBox()
    {
        Gears.Add(-3.76f); //Reverse
        Gears.Add(0f); // Neutral
        Gears.Add(3.136f); //1st
        Gears.Add(1.888f); //2nd
        Gears.Add(1.33f); //3rd
        Gears.Add(1f); //4th
        Gears.Add(0.814f); //5th

        FinalDifferential = 4.1f;
    }

    /// <summary>
    /// Sets up torque curve lookup
    /// </summary>
    public void SetTorqueCurve()
    {
        TorqueCurve.AddKey(0, 0);
        TorqueCurve.AddKey(1000, 65);
        TorqueCurve.AddKey(5500, 135);
        TorqueCurve.AddKey(7250, 0);
    }

    // Update is called once per frame
    void Update()
    {
        RPM = ((RL.rpm + RR.rpm) / 2) * Gears[CurrentGear] * FinalDifferential;

        RPM = Mathf.Clamp(RPM, 1000, 7250);

        Throttle = Mathf.Clamp(Throttle, 0, 1);

        EngineTorque = TorqueCurve.Evaluate(RPM);

        WheelTorque = EngineTorque * Gears[CurrentGear] * FinalDifferential * Throttle;

        RL.motorTorque = WheelTorque / 2;
        RR.motorTorque = WheelTorque / 2;

        CurrentGear = OptimumGear();

        Speed = GetComponent<Rigidbody>().velocity.magnitude * 2.23694f;

        if (Speed < 62)
        {
            ZTSTimer += Time.deltaTime;
        }
    }

    public int OptimumGear()
    {
        List<float> testGearTorqueFloats = new List<float>();

        for (var gearIndex = 0; gearIndex < Gears.Count; gearIndex++)
        {
            float testRPM = Mathf.Clamp(((RL.rpm + RR.rpm) / 2) * Gears[gearIndex] * FinalDifferential, 1000, 7250);
            float testEngineTorque = TorqueCurve.Evaluate(testRPM);
            testGearTorqueFloats.Add(testEngineTorque * Gears[gearIndex] * FinalDifferential * Throttle);
        }

        return testGearTorqueFloats.IndexOf(testGearTorqueFloats.Max());
    }


    //AI
    public override void OnEpisodeBegin()
    {
        rBody.angularVelocity = Vector3.zero;
        rBody.velocity = Vector3.zero;
        transform.localPosition = new Vector3(0, 01, 0);
    }

    public override void CollectObservations(VectorSensor sensor)
    {

    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {

    }

    //The Heuristic function allows to manually control the actions of the agent to test it by hand before handing it over the ML AI
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
}