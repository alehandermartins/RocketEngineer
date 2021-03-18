using System;
using System.Collections.Generic;
using UnityEngine;

public class FlightParams
{
    public float RadiansTravelled { get; set; }
    public float Acceleration { get; set; }
    public float Direction { get; set; }
    public string FlyState { get; set; }
    public Vector3 Speed { get; set; }
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
    public Vector3 PlanetPosition { get; set; }
    public Vector3 StartPoint { get; set; }
    public Vector3 SubOrbitCenter { get; set; }
    public float TurningPoint { get; set; }
    public float OrbitAngle { get; set; }
    public float FlyByAngle { get; set; }
    public string[] Maneuvers { get; set; }
    public float ManeuverStart { get; set; }
}
