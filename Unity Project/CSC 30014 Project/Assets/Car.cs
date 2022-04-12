using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEditor;
using UnityEngine;

public class Car : Agent
{
    //Track
    public List<GameObject> TrackPieces = new List<GameObject>();
    public List<GameObject> Checkpoints = new List<GameObject>();
    public int CurrentCheckpoint;

    //Inputs
    public float Steering;
    public float Throttle;
    public float Brake;

    //Car Stuff
    public AnimationCurve TorqueCurve;
    public List<float> Gears = new(); //Index 0 and 1 are reserverd for reverse and neutral, index 1 must = 0 for neutral;
    public float WheelTorque;
    public float EngineTorque;
    public int CurrentGear;
    public float FinalDifferential;
    public float RPM;

    public WheelCollider FL;
    public WheelCollider FR;
    public WheelCollider RL;
    public WheelCollider RR;

    public float Speed;
    public float ZTSTimer;
    public float FrontBrakePeakNM;
    public float RearBrakePeakNM;
    
    private Rigidbody rBody;
    private float ZeroSpeedTimer;

    public GameObject _findCheckpoint1After;
    public GameObject _findCheckpoint2After;
    public GameObject _findCheckpoint3After;
    public GameObject _findCheckpoint4After;

    // Start is called before the first frame update
    public override void Initialize()
    {
        rBody = GetComponent<Rigidbody>();

        //Set up engine
        SetTorqueCurve();
        SetupGearBox();

        //Set Brakes
        //1400nm is the peak Brake force for the Mazda
        FrontBrakePeakNM = 1400 * 0.67f;
        RearBrakePeakNM = 1400 * (1 - 0.67f);

        ZeroSpeedTimer = 5;
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
    void FixedUpdate()
    {
        RPM = ((RL.rpm + RR.rpm) / 2) * Gears[CurrentGear] * FinalDifferential;

        RPM = Mathf.Clamp(RPM, 1000, 7250);

        Throttle = Mathf.Clamp(Throttle, 0, 1);

        EngineTorque = TorqueCurve.Evaluate(RPM);

        WheelTorque = EngineTorque * Gears[CurrentGear] * FinalDifferential * Throttle;

        CurrentGear = OptimumGear();

        FL.steerAngle = Steering * 30;
        FR.steerAngle = Steering * 30;

        RL.motorTorque = WheelTorque / 2;
        RR.motorTorque = WheelTorque / 2;

        FL.brakeTorque = FrontBrakePeakNM * Brake;
        FR.brakeTorque = FrontBrakePeakNM * Brake;
        RL.brakeTorque = RearBrakePeakNM * Brake;
        RR.brakeTorque = RearBrakePeakNM * Brake;

        if (rBody.velocity.magnitude < 1)
        {
            ZeroSpeedTimer -= Time.fixedDeltaTime;
        }

        if (ZeroSpeedTimer <= 0)
        {
            SetReward(-1f);

            EndEpisode();
        }

        TestingThings();
    }

    private void TestingThings()
    {
        Speed = GetComponent<Rigidbody>().velocity.magnitude * 2.23694f;

        if (Speed < 62)
        {
            ZTSTimer += Time.deltaTime;
        }
    }

    /// <summary>
    /// Finds the optimum gear to use based on the cars current speed conditions
    /// </summary>
    /// <returns></returns>
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
        //GenerateTrack();

        FindCheckpointsFromTrackPieces();

        CurrentCheckpoint = -1;

        rBody.angularVelocity = Vector3.zero;
        rBody.velocity = Vector3.zero;

        transform.localPosition = new Vector3(0, 0.6f, 0); //Height of car from top to bottom of tyre with fully extended suspension
        transform.localEulerAngles = Vector3.zero;

        ZeroSpeedTimer = 2;
    }

    public void FindCheckpointsFromTrackPieces()
    {
        Checkpoints.Clear();

        foreach (var trackPiece in TrackPieces)
        {
            var allKids = GetComponentsInChildren<Transform>();

            Transform[] ts = trackPiece.transform.GetComponentsInChildren<Transform>();

            foreach (Transform t in ts)
            {
                if (t.gameObject.name == "Checkpoint1")
                {
                    Checkpoints.Add(t.gameObject);
                }
            }

            foreach (Transform t in ts)
            {
                if (t.gameObject.name == "Checkpoint2")
                {
                    Checkpoints.Add(t.gameObject);
                }
            }
        }
    }

    public void GenerateTrack()
    {
        foreach (var trackPiece in TrackPieces)
        {
            Destroy(trackPiece);
        }
    }

    //Feed info/obsevations into AI
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position);
        sensor.AddObservation(transform.rotation);

        sensor.AddObservation(rBody.velocity);
        sensor.AddObservation(rBody.angularVelocity);

        _findCheckpoint1After = FindCheckpointAfter(CurrentCheckpoint, 1);
        sensor.AddObservation(_findCheckpoint1After.transform.position);
        sensor.AddObservation(_findCheckpoint1After.transform.rotation);
        _findCheckpoint2After = FindCheckpointAfter(CurrentCheckpoint, 2);
        sensor.AddObservation(_findCheckpoint2After.transform.position);
        sensor.AddObservation(_findCheckpoint2After.transform.rotation);
        _findCheckpoint3After = FindCheckpointAfter(CurrentCheckpoint, 3);
        sensor.AddObservation(_findCheckpoint3After.transform.position);
        sensor.AddObservation(_findCheckpoint3After.transform.rotation);
        _findCheckpoint4After = FindCheckpointAfter(CurrentCheckpoint, 4);
        sensor.AddObservation(_findCheckpoint4After.transform.position);
        sensor.AddObservation(_findCheckpoint4After.transform.rotation);
    }

    public GameObject FindCheckpointAfter(int currCheckpoint, int addAmount)
    {
        int finalIndexInArray = Checkpoints.Count - 1;

        while (addAmount > 0)
        {
            currCheckpoint++;

            if (currCheckpoint > finalIndexInArray)
            {
                currCheckpoint = 0;
            }

            addAmount--;
        }

        return Checkpoints[currCheckpoint];
    }

    //Apply AI inputs and rewards
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        Steering = Mathf.Clamp(actionBuffers.ContinuousActions[0], -1, 1);

        Throttle = Mathf.Clamp(actionBuffers.ContinuousActions[1], 0 ,1);
        Brake = -Mathf.Clamp(actionBuffers.ContinuousActions[1], -1, 0);

        //AddReward(0.001f * rBody.velocity.magnitude);
    }

    //The Heuristic function allows to manually control the actions of the agent to test it by hand before handing it over the ML AI
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Wall")
        {
            SetReward(-1f);

            EndEpisode();
        }
    }

    public void OnTriggerEnter(Collider collision)
    {
        var findCheckpointAfter = FindCheckpointAfter(CurrentCheckpoint, 1);
        if (collision.gameObject == findCheckpointAfter)
        {
            AddReward(1f);

            AddReward(0.1f * rBody.velocity.magnitude);

            CurrentCheckpoint = Checkpoints.IndexOf(collision.gameObject);
        }

        else
        {
            SetReward(-1f);

            EndEpisode();
        }
    }
}