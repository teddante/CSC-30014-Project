
# CSC-30014-Project: Using Machine Learning to Determine the Optimum Racing Line

*Edward D. P. Enston*

# Abstract
This is a project about simplifying someone&#39;s experience of finding the fastest way around a racetrack. This is called the racing line and it allows a driver to carry the most speed around a racetrack to achieve the shortest time. Finding a racing line around a track can be quite a hard challenge, an experienced driver can find it in a few laps, so can we use machine learning and reinforcement learning to model this behavior and find a racing line like an experienced driver would find. This project logs my attempt to use the 3D software framework Unity and its machine learning component to find the fastest line around a track while providing a sufficient simulation of a car.

_Keywords_: simulation, machine learning, racing, car, racing line

# Using Machine Learning to Determine the Optimum Racing Line

This project revolves around using different machine learning approaches to train an AI to find the best racing line around a circuit to find the optimum fastest racing line around the track where the AI can take the most speed around the track to gain the fastest possible time. A racing line in motorsport is used to make the most use of the width of a racetrack to minimize the lap time of a track. A vague way to determine the racing line of a track is to attempt to draw the largest radius circle around each corner of the track to maximize speed because a larger radius around a corner requires less of a speed decrease. But this method often looks over the complexities of vehicle dynamics which affect the shape of this large radius circle though it does get into a good ballpark. We want to investigate optimizing this shape around a corner for maximum speed.

Inspiration for this comes from my interest in motorsport and vehicle dynamics simulations. I grew up watching Formula 1 and playing Formula 1 simulation on different software such as rFactor and the Codemasters F1 series. These simulations inspired me to emulate these simulations, so I worked on small personal projects based around vehicle dynamics simulations. I also messed around with implementations of AI drivers and how they find the optimum racing line on the tracks within the simulations. I struggled to find a way to compute these racing lines algorithmically so I resorted to hand-drawn lines which they would treat as the optimum line and would work around that line. But with the large recent developments in machine learning, I realized I could seek these optimum racing lines with machine learning techniques to automatically find a racing line for the AI through an experience like a human does instinctively overtime to find an optimal racing line.

This will be done in a 3D software framework called Unity. Here we can use the software to program a basic car simulation that the AI will use and then we will program and set up different machine learning approaches that we can evaluate. We will be able to implement data collection into this program which will interface with Unity&#39;s ML-Agents.

# Report

# Simulation and Environment

## Installing all required software

For this project I needed several bits of software to get this project up and running. I need to install the Unity 2020.3 or later along with Python 3.6.1 or later. I opted for the latest beta release of unity at 2022.1.0b6 and Python 3.7. I also need to clone the entire ML-Agents Toolkit repository from Unity Technologies to ensure I have all the features within the ML framework. From here I installed the Unity ML-Agents Unity package and its extensions along with the ML-Agents Python package. I made sure PyTorch 1.7.1 was installed as well as it&#39;s a prerequisite for the ML-Agents Python package.

## Unity

Unity is a platform for creating and operating interactive, real-time 3D content, it&#39;s used very commonly in game development and for simulations in different fields like engineering and robotics, especially with the rise in machine learning and it&#39;s new &#39;Machine Learning Agents&#39; module which has allowed to be used in machine learning simulations. (Unity Technologies, n.d.)This was what led me to use the Unity platform for this machine learning experiment along with my prior experience with using Unity to make vehicle simulations since I was fourteen.

I am going to create a Unity project for this project using Unity version &#39;2022.1.0b6.&#39; This project will be source controlled within GitHub to prevent data loss and allows for version control and track change history.

## Vehicle Implementation

For testing purposes, I am going to implement a basic vehicle simulation of a common basic sports car design. I am going to create a vehicle like a front-engine, rear-wheel-drive roadster, like a Mazda MX-5 for example. (Ultimate Specs, n.d.) This means I have a solid foundation for information based on a vehicle of this type when looking for specifications, like mass, gearbox ratios or suspension spring rates, for example, related to a roadster so the simulation is stable at startup with minimal hassle attempting to fine-tune vehicle specifications values from thin air while attempting to simulate a basic vehicle. This design of the vehicle is also basic and easy to work with because it is very barebones.

### Tire Model

I am using Unity&#39;s Wheel Collider to model wheel and tire physics. For the tire friction modelling it uses a slip-based tire friction model. (Unity Technologies, 2022) This tire friction model is like the widely used Pacejka Magic Formula tire model. The Magic Formula model is used widely when modelling something as complex as a tire and how it functions because of how efficient it is, and it accurately covers, and models tire physics closely enough to be used in lots of vehicle simulations.

The Magic Formula tire model is based on a curve where the force outputted by the tire to the car is determined by the slip of the tire. This slip can be either be in the sideways direction meaning that the slip is based on the angle of the cars travel of direction relative to the direction that the tire described is face, or it can be forward slip where the wheel is rotating faster than the surface speed of the tire. The curve for slip-force follows something like the image below.

The Unity Wheel collider also includes a simple spring-damper model for modelling suspension which is more than enough for our usage.

### Car Specifications

I have compiled specifications for a vehicle we are going to model with simplified and averaged values from numerous sources. Below is a table with the contents of the car&#39;s specifications. These values will be explained further below.

#### General Car Specifications

| Car Specification | Value |
| --- | --- |
| Vehicle Curb Mass (kg) | 950 kg |
| --- | --- |
| Engine Maximum Torque (Nm) (rpm) | 135 Nm @ 5000 rpm |
| Engine Limiter (rpm) | 7250 rpm |
| Suspension Spring Rate (N/m) | 19300 N/m |
| Suspension Damping Ratio (Dimensionless) | 2244 |
| Total Wheel Radius (m) | 0.2888 |

_Table 1 – Car Specification_

These values represent the most important components of the vehicle&#39;s handling and power. The engine&#39;s maximum torque represents the maximum amount of torque that the engine delivers at a specified rotation speed of the engine output shaft. We will use this value to determine a vague torque curve of the engine. Internal combustion engine torque vaguely follows a curve which has a peak where the engine outputs a peak amount of torque at a certain rotational speed of the engine. Here below is an example torque curve of an engine like the one we are modelling:

![Screenshot 2022-01-16 133223](https://user-images.githubusercontent.com/37670093/175773412-cee9ab32-3d15-44c6-b1db-fe66116496b2.png)

_Figure 1 – Car Torque Curve_

The driver will manipulate the gearbox to keep the engine rotation speed as close to the peak of the torque curve as much as possible.

The front and rear suspension values are detailed in the table, these values are like values you would find in a 950kg roadster, these values will be placed in the simulation to model the dynamics of the vehicle under load during braking, cornering, and accelerating such as weight transfer and body roll for example.

#### Gearbox Ratios

Here I am detailing the gear ratios used in the vehicle. These values are dimensionless and are used to allow for torque conversions to reach higher speeds within the engines torque curve. The AI will not have to manage gear changes as it will be operated by a simple algorithm that will search through all the gears to determine the highest possible torque that can be accessed at the vehicles current speed.

| Gear | Ratio |
| --- | --- |
| Reverse | -3.76 |
| --- | --- |
| 1 | 3.136 |
| 2 | 1.888 |
| 3 | 1.33 |
| 4 | 1 |
| 5 | 0.814 |
| Final Differential | 4.1 |

_Table 2 – Gearbox Specification_

### Drivetrain Modelling

The drivetrain modelling is quite simple and is modelling by simple gear ratios. We are not modelling a clutch between the engine and gearbox or a differential after the gear box to the rear axle simplifying it would be over engineering for the purposes of this model. This means there is a direct connection in the torque between the engine, gearbox, and differential where changes between gears happen instantaneously without loss of power due to perfect efficiency in this theoretical drivetrain. I have created an algorithm that the AI will use to determine the optimum gear that will provide the most torque at that point in time. This is done to reduce complexity in the ML process. Below is the code for the drive train modelling that I put together; this is called every physics step within the simulation: 

    RPM = ((RL.rpm + RR.rpm) / 2) * Gears[CurrentGear] * FinalDifferential;
    
    RPM = Mathf.Clamp(RPM, 1000, 7250);
    
    Throttle = Mathf.Clamp(Throttle, 0, 1);
    
    EngineTorque = TorqueCurve.Evaluate(RPM);
    
    WheelTorque = EngineTorque * Gears[CurrentGear] * FinalDifferential * Throttle;
    
    CurrentGear = OptimumGear();
    
    RL.motorTorque = WheelTorque / 2;
    RR.motorTorque = WheelTorque / 2;

_Figure 2 - Drive train algorithm_

#### Optimum Gear Algorithm

I have created an algorithm to find the optimum gear to use based on the cars rear axle speed which is based on the rear tire&#39;s rotation speed. Below is the function for the algorithm. It iterates through a which includes all the torques values of each gear based on the car&#39;s current tire rotation speed. The index of the max value of this array is returned as the optimum gear based on the current scenario to give the wheel&#39;s most torque at that current point. This function is called with every time the AI decides an Action.


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

_Figure 3 – Code for finding the optimum gears_

### Aerodynamics

Very barebones aerodynamics modelling will take place in the simulation due to the aerodynamics very a bit of a negligible effect on the car compared to an open wheeler race car for example, and it reduces the complexity of the model to simplify it. The only aerodynamics-based force that will have a large effect on the car will be basic drag so this will be implemented.

### Car Visual 3D Model

I am also going to create a basic visual model of the vehicles body just to represent the volume it takes up in 3D space. I am using Blender to create this model which is exported for use in Unity. It vaguely resembles a simple two-passenger roadster sports car.

![Picture3](https://user-images.githubusercontent.com/37670093/175773596-36a23969-dfef-472a-9f07-dc2e6bb6784c.png)

_Figure 4 - Screen capture of Unity and the basic car model in the track with checkpoints_

## Static Track Design

For the first part of the project, I am going to work on the machine learning environment within a static track to keep it simple. The track will be short to allow for quick iteration between laps. It features a few of the classic track features that are usually within your usual racetracks such as hairpins, chicanes, and basic left right turns. Here is an image of the track I&#39;ve created within Unity that I will initially use for testing purposes.

![Picture2](https://user-images.githubusercontent.com/37670093/175773613-cb654851-814d-4b1f-80e0-a9d184fb833f.png)

_Figure 5 – Static Track Design example_

It includes track features mentioned prior. Each track segment is composed of a 20m by 20m block which composed of 3 shapes, a straight, a left and a right shape which can be put together to compose a full track.

## Unity Machine Learning Agents

### Observations

Our reinforcement learning agent will be trained on three core ideas, observations, actions, and reward signals. Essentially giving our AI a pair of eyes. We will first discuss the observations that the agent uses to perceive the environment, using the information based on its observations, it will decide and then act on that decision, hopefully gaining a reward in the process. Observations can only be numerical based types of data like floats, integers and Booleans while also including anything that can be broken down into numerical components like a vector or an image the numerical color value of each pixel for example but this means a large amount of data is being processes so it&#39;s best to keep the number of observations as low as possible while giving sufficient information. Here are our observations we are going to use and their descriptions for our use:

- Car position – This will be used to determine the car&#39;s position within the 3D space and can be used by the agent to determine its position relative to the track&#39;s checkpoints.
- Car rotation – The car&#39;s rotation is used to determine where the car is facing and its relation to the direction of the checkpoints within the track. The agent can use the car&#39;s pitch and roll to determine the state of car&#39;s body roll as the car shifts about when its acceleration changes and the weight of the car shifts around.
- Car velocity – The agent may use this observation to calculate whether it is going to fast or too slow to reach it destination safely and fast.
- Car angular velocity – This is like the car rotation observation but can be better used to determine body roll or whether the car is rotating enough to reach a checkpoint and it may use the throttle and brake to manipulate the car&#39;s body roll to gain more oversteer or understeer.
- The position and rotation of the next four upcoming checkpoint – The agent will use this to determine its position and rotation relative to the upcoming checkpoints, it can use this information to prepare for the next few corners and get into the right position for them to reach the highest possible safe speed for the corners.
- Sixteen depth sensors around the car – These are sixteen sensors that measure the distance of objects around the car, these will be used to detect nearby object that the car will hopefully avoid a collision.

This is a total of forty-one floats that are being observed by the agent. Below is the function for colling the observations for the agent:

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

_Figure 6 - Code for collecting observations_

The function above includes a function called &#39;FindCheckpointAfter&#39; which is used to loop around the array of checkpoints to find the next checkpoint and several after which can be specified with the second parameter of the function. The function is described below:


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

_Figure 7 – Algorithm for the looping through the array of checkpoints to find the next checkpoint, notice it allows for looping around to the start_

### Decisions

The agent makes decisions at set intervals within the simulation. I have control of the rate that the decisions are made and its important to find a nice balance making enough decisions to sufficiently take appropriate action of the car and the scenarios its in while also moderating the decision rate to keep it low enough that training can occur quick as it uses processing power to make a decision as this is where the network and policy is analyzed. I decided on a decision being made every ten steps in the simulation as adequate.

### Actions

This is the stage the decisions of the network are made to actions. Actions within the framework can be only continuous or discrete. We will be using two continuous values for the simulation, and these will represent the steering and the throttle/brake of the car. Below is the algorithm for the mapping of the actions and clamping them for use as the car&#39;s inputs:

    Steering = Mathf.Clamp(actionBuffers.ContinuousActions[0], -1, 1);
    
    Throttle = Mathf.Clamp(actionBuffers.ContinuousActions[1], 0 ,1);
    Brake = -Mathf.Clamp(actionBuffers.ContinuousActions[1], -1, 0);

_Figure 8 - Mapping of the actions and clamping them for use as the car&#39;s inputs_

### Rewards

The reward function of this project was incredibly hard to calculate to get the results I was looking for. The reward function I settled on based on my research was as follows:

- Giving a reward when crossing the correct upcoming checkpoint of the track – This was used to give the agent an idea of the tracks orientation and a goal to work towards.
- Giving a reward for the speed as it crossed a checkpoint – This reward is scaled with the magnitude of the velocity vector; this was done with the hope of increasing the velocity of the agent and press the agent to find the optimum line around the track to maximize speed.
- Punishment for hitting a wall – This is to prevent collision with the walls, also the episode is ended when a wall is hit. 
```
public void OnCollisionEnter(Collision collision)
{
    if (collision.gameObject.name == "Wall")
    {
        SetReward(-1f);

        EndEpisode();
    }
}
```

_Figure 9 – Algorithm for colliding with a wall and its punishment and ending of the episode_

- Punishment for hitting the wrong checkpoint – This will prevent the agent from back tracking into a prior checkpoint and losing itself on the track, hopefully pressuring the agent to keep in the right flow of the track.
```
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
```
_Figure 10 - Algorithm for hitting a checkpoint and checking if its the right checkpoint_

- Punishment for coming to a standstill – This prevents the agent from coming to a standstill and allowing an episode to play out for a large number of steps with no progress. This also resets the episode. See figure 8. 
```
    if (rBody.velocity.magnitude < 1)
    {
        ZeroSpeedTimer -= Time.fixedDeltaTime;
    }
    
    if (ZeroSpeedTimer <= 0)
    {
        SetReward(-1f);
    
        EndEpisode();
    }
```
_Figure 11 - Checking for a standstill, called every academy step_

## Environment

The track environment mentioned earlier is used for each episode. When episode begins where the agent is trained, all the checkpoints from the track pieces are quickly thrown into an array to save time. Also, the car is reset to its starting position and direction while is velocity and angular velocity are reset to zero. ![](RackMultipart20220625-1-cc6wv7_html_c30ef9e9d7465aca.gif)

_Figure 12 - Algorithm used at the start of each episode_

Here below is the code for finding all of the checkpoints within the track pieces: ![](RackMultipart20220625-1-cc6wv7_html_3a100f26800a2e68.gif)

_Figure 13 - Algorithm used to find all the checkpoints within the track_

Evaluation

This project was a lot more challenging and complex than I initially thought when I proposed it. This was proved when I went through numerous steps to correct the reward function and the observations trying to get the agent to get around the track correctly. What occurred was that the agent attempted for execute the first turn as fast as possible, I assumed this would have been since a lot of the reward function was based on the speed of the agent. This meant I went back though to edit the reward function to take out the involvement with velocity and to only reward based on progressing through the checkpoint. However, this meant the agent struggled to pull away from a stand still so velocity had to be reimplemented. ![](RackMultipart20220625-1-cc6wv7_html_4c71a76b634555a0.png)

_Figure 14 - Tensor Board of the result_

Above is the Tensor Board of the final test that I left to run for 1.4 million steps. This took several hours, and the outcome was an agent that could execute 1 corner incredibly fast, before either colliding with the wall or coming to a standstill where the anti-standstill code kicked in. As you can see the cumulative reward tapered off where it slowly reaches what it thinks is the right outcome.

Conclusion

The project turned out to be a failure. The program went thorough numerous iterations to hopefully push the agent on track around the track towards the goal of finding the optimum racing line around a track. This occurred because I did not recognize the size and scale of a project like this and it being incredibly complex.

References

Auckley, J., Horenstein, M., Esber, Z., &amp; Donohue, J. (2019, July 26). _Genetic Programming for Racing Line Optimization- Part 1 | by Joe Auckley | Adventures in Autonomous Vehicles_. Retrieved from Medium: https://medium.com/adventures-in-autonomous-vehicles/genetic-programming-for-racing-line-optimization-part-1-e563c606e502

CodeReclaimers. (n.d.). _NEAT Overview — NEAT-Python 0.92 documentation_. Retrieved from NEAT-Python: https://neat-python.readthedocs.io/en/latest/neat\_overview.html

Fuchs, F., Song, Y., Kaufmann, E., Scaramuzza, D., &amp; Durr, P. (2021, February). _Super-Human Performance in Gran Turismo Sport Using Deep Reinforcement Learning._ Retrieved from Robotics and Perception Group: http://rpg.ifi.uzh.ch/docs/RAL21\_Fuchs.pdf

Garlick, S., &amp; Bradley, A. (2021, December 17). _Real-time optimal trajectory planning for autonomous vehicles and lap time simulation using machine learning_. Retrieved from https://www.tandfonline.com/doi/full/10.1080/00423114.2021.2011929

Ghimire, M. (2021, May 1). _A Study of Deep Reinforcement Learning in Autonomous Racing Using DeepRacer Car_. Retrieved from eGrove: https://egrove.olemiss.edu/cgi/viewcontent.cgi?article=2782&amp;context=hon\_thesis

Giancaterino, C. G. (2019, July 8). _ANNA — Artificial Neural Network Agent for MotoGP™ 19 | by Data Science Milan | Medium_. Retrieved from Data Science Milan: https://datasciencemilan.medium.com/a-n-n-a-artificial-neural-network-agent-for-motogp-19-f554fbe9ca

Gonzalez, D. (2020, June 15). _An Advanced Guide to AWS DeepRacer | by Daniel Gonzalez_. Retrieved from Towards Data Science: https://towardsdatascience.com/an-advanced-guide-to-aws-deepracer-2b462c37eea

Jain, A., &amp; Morari, M. (2020, Febuary 12). _Computing the racing line using Bayesian optimization_. Retrieved from arXiv: https://arxiv.org/pdf/2002.04794.pdf

Klapálek, J., Novák, A., Sojka, M., &amp; Hanzálek, Z. (2021, December 16). _Car Racing Line Optimization with Genetic Algorithm using Approximate Homeomorphism_. Retrieved from IEEE Xplore: https://ieeexplore.ieee.org/abstract/document/9636503

Nebol&#39;s Coding. (2019, January 4). _Neural network racing cars around a track_. Retrieved from YouTube: https://www.youtube.com/watch?v=wL7tSgUpy8w

Nosowsky, C., &amp; Smith, K. (n.d.). _Evolutionary Approach to Finding an Optimal Racing Line in a Vehicle Simulator._ Retrieved from https://chrisnosowsky.com/files/Evolutionary%20Approach%20to%20Finding%20an%20Optimal%20Racing%20LIne%20in%20a%20Vehicle%20Simulator.pdf

Pacejka, H., &amp; Besselink, I. M. (2012). _Tire and Vehicle Dynamics._ Elsevier Science.

Race Optimal, &amp; Vesel, R. (n.d.). _Race Optimal_. Retrieved from https://www.raceoptimal.com/

Remonda, A., Krebs, S., Veas, E., Luzhnica, G., &amp; Kern, R. (2021, April 22). _Formula RL: Deep Reinforcement Learning for Autonomous Racing using Telemetry Data._ Retrieved from arXiv: https://arxiv.org/pdf/2104.11106.pdf

Tipping, M. E., Hatton, A., Herbrich, R., &amp; Microsoft. (2011, Febuary 22). _RACING LINE OPTIMIZATION._ Retrieved from United States Patent: https://patentimages.storage.googleapis.com/89/7d/08/ba96f5384de062/US7892078.pdf

Ultimate Specs. (n.d.). _Mazda MX 5 Miata (NA) 1.6i Technical Specs, Dimensions_. Retrieved from Ultimate Specs: https://www.ultimatespecs.com/car-specs/Mazda/7593/Mazda-MX-5-Miata-(NA)-16i.html

Unity Technologies. (2022, March 19). _Manual: Wheel Collider._ Retrieved from Unity - Manual: https://docs.unity3d.com/2022.2/Documentation/Manual/class-WheelCollider.html

Unity Technologies. (n.d.). _Unity-Technologies/ml-agents: Unity Machine Learning Agents Toolkit_. Retrieved from GitHub: https://github.com/Unity-Technologies/ml-agents

Xiong, Y. (2010, July 30). _Racing Line Optimization_. Retrieved from https://dspace.mit.edu/bitstream/handle/1721.1/64669/706825301-MIT.pdf

Zal, P. (n.d.). _Horsepower and Torque curve for 1998 Mazda MX-5 1.6 (man. 5) offered since April 1998 for Europe_. Retrieved from Automobile Catalog: https://www.automobile-catalog.com/curve/1998/1667015/mazda\_mx-5\_1\_6.html

Appendix

Source code:

[https://github.com/teddante/CSC-30014-Project](https://github.com/teddante/CSC-30014-Project)
