using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketController : MonoBehaviour
{
    public float speed = 0.0f;
    Vector3 speedVector;
    float acceleration = 0.01f;

    float mass = 1f;

    Vector3 bodyUp = new Vector3(-1, 0, 0);
    Vector3 bodyForward = new Vector3(0, 0, 1);

    Vector3 currentPlanetPosition = new Vector3(0, 0, 0.5f);
    Vector3 planetPosition;
    Vector3 targetPlanetPosition = new Vector3(0, 0, 2);
    Vector3 startPoint = new Vector3(-0.11f, 0, 0);

    float orbitRadius = 0.3f;
    float subOrbitRadius = 0.1f;

    Vector3 subOrbitCenter;
    float turningPoint;
    float orbitAngle;
    float flyByAngle;

    float height = 0;

    float radiansTravelled = 0.0f;
    float direction = 0;

    public string[] maneuvers = { };
    public string flyState = "landed";

    float maneuverStart = 0;

    private List<GameObject> capsules = new List<GameObject>();
    private List<GameObject> thrusters = new List<GameObject>();
    private List<ThrusterController> thrusterControllers = new List<ThrusterController>();

    private ThrusterController thrusterController;

    public float capsuleHeight = 0.00838f;

    FlightParams flightParams = new FlightParams
    {
        Speed = new Vector3(),
        RadiansTravelled = 0,
        Acceleration = 0,
        Direction = 0,
        FlyState = "landed",
        Position = new Vector3(),
        Rotation = new Quaternion(),
        PlanetPosition = new Vector3(),
        StartPoint = new Vector3(),
        SubOrbitCenter = new Vector3(),
        TurningPoint = 0,
        OrbitAngle = 0,
        FlyByAngle = 0,
        Maneuvers = new string[] { },
        ManeuverStart = 0
    };

    LandToOrbitManeuver landToOrbit = new LandToOrbitManeuver();
    OrbitToLandManeuver orbitToLand = new OrbitToLandManeuver();
    OrbitToFlyByManeuver orbitToFlyBy = new OrbitToFlyByManeuver();
    FlyByToOrbitManeuver flyByToOrbit = new FlyByToOrbitManeuver();

    public void AddCapsule(GameObject capsule)
    {
        capsules.Add(capsule);
        capsule.transform.parent = transform;
        height += capsuleHeight;
        capsule.transform.localPosition = new Vector3(0,1,0) * capsuleHeight / 2;
        capsule.transform.localRotation = Quaternion.identity;
    }

    public void AddThruster(GameObject thruster)
    {
        thrusterController = thruster.GetComponent<ThrusterController>();

        foreach (GameObject capsule in capsules)
        {
            capsule.transform.localPosition += new Vector3(0, 1, 0) * thrusterController.height;
        }

        foreach (GameObject _thruster in thrusters)
        {
            _thruster.transform.localPosition += new Vector3(0, 1, 0) * thrusterController.height;
        }

        thrusters.Add(thruster);
        thrusterControllers.Add(thrusterController);
        thruster.transform.parent = transform;
        height += thrusterController.height;
        thruster.transform.localPosition = new Vector3(0, 1, 0) * thrusterController.height / 2;
        thruster.transform.localRotation = Quaternion.identity;
    }

    private void Attach(GameObject obj)
    {
        height += capsuleHeight;
        transform.position += bodyUp * height / 2;

        obj.transform.parent = transform;
        obj.transform.localPosition = new Vector3(0, 0, 0);
        obj.transform.localRotation = Quaternion.identity;
    }

    public GameObject Dettach()
    {
        GameObject dettached = thrusters[thrusters.Count - 1];
        thrusters.RemoveAt(thrusters.Count - 1);
        thrusterControllers.RemoveAt(thrusterControllers.Count - 1);

        foreach (GameObject capsule in capsules)
        {
            capsule.transform.localPosition -= new Vector3(0, 1, 0) * thrusterController.height;
        }

        foreach (GameObject _thruster in thrusters)
        {
            _thruster.transform.localPosition -= new Vector3(0, 1, 0) * thrusterController.height;
        }

        thrusterController = thrusterControllers[thrusterControllers.Count - 1];

        return dettached;
    }

    public FlightParams GetFlightParams()
    {
        return new FlightParams
        {
            Speed = speedVector,
            RadiansTravelled = radiansTravelled,
            Acceleration = acceleration,
            Direction = direction,
            FlyState = flyState,
            Position = transform.position,
            Rotation = transform.rotation,
            PlanetPosition = planetPosition,
            StartPoint = startPoint,
            SubOrbitCenter = subOrbitCenter,
            TurningPoint = turningPoint,
            OrbitAngle = orbitAngle,
            FlyByAngle = flyByAngle,
            Maneuvers = maneuvers,
            ManeuverStart = maneuverStart
        };
    }

    public void SetFlightParams(FlightParams flightParams)
    {
        speed = flightParams.Speed.magnitude;
        speedVector = flightParams.Speed;
        radiansTravelled = flightParams.RadiansTravelled;
        acceleration = flightParams.Acceleration;
        direction = flightParams.Direction;
        flyState = flightParams.FlyState;
        transform.position = flightParams.Position;
        transform.rotation = flightParams.Rotation;
        planetPosition = flightParams.PlanetPosition;
        startPoint = flightParams.StartPoint;
        subOrbitCenter = flightParams.SubOrbitCenter;
        turningPoint = flightParams.TurningPoint;
        orbitAngle = flightParams.OrbitAngle;
        flyByAngle = flightParams.FlyByAngle;
        maneuvers = flightParams.Maneuvers;
        maneuverStart = flightParams.ManeuverStart;
    }

    public void EnableBooster()
    {
        if (thrusterControllers.Count == 0) return;
        thrusterControllers[0].EnableBooster();
    }

    public void DisableBooster()
    {
        if (thrusterControllers.Count == 0) return;
        thrusterControllers[0].DisableBooster();
    }

    public void SetDirections()
    {
        planetPosition = currentPlanetPosition;
        transform.position = planetPosition + startPoint;
        transform.eulerAngles = new Vector3(0, 0, 90);

        flightParams.PlanetPosition = currentPlanetPosition;
        flightParams.Position = planetPosition + startPoint;
        flightParams.Rotation = transform.rotation;
    }

    private void SetLaunchOrbits()
    {
        SetOrbitStart();

        subOrbitCenter = bodyUp * turningPoint - bodyForward * subOrbitRadius;
        float angleDeg = Vector3.Angle(bodyUp, (subOrbitCenter - new Vector3(0, 0, 0)).normalized);
        orbitAngle = angleDeg * Mathf.PI / 180.0f;

        AttachOrbitToPlanet();

        float height = turningPoint - startPoint.magnitude + (orbitAngle + Mathf.PI / 2) * subOrbitRadius;
        thrusterController.SetFlowRate(height);
    }

    private void SetLandingOrbits()
    {
        SetOrbitStart();

        subOrbitCenter = bodyUp * turningPoint + bodyForward * subOrbitRadius;
        float angleDeg = Vector3.Angle(bodyUp, (subOrbitCenter - new Vector3(0, 0, 0)).normalized);
        orbitAngle = (90 - angleDeg) * Mathf.PI / 180.0f;

        AttachOrbitToPlanet();
    }

    private void SetFlyByOrbit()
    {
        if (planetPosition == currentPlanetPosition) flyByAngle = Mathf.PI;
        else flyByAngle = 2 * Mathf.PI;
    }

    private void SetOrbitStart()
    {
        turningPoint = Mathf.Sqrt(Mathf.Pow(orbitRadius - subOrbitRadius, 2) - Mathf.Pow(subOrbitRadius, 2));
    }

    private void AttachOrbitToPlanet()
    {
        subOrbitCenter += planetPosition;
    }

    private void ResetOrbit()
    {
        if (radiansTravelled >= 2 * Mathf.PI)
        {
            radiansTravelled = roundRadians(radiansTravelled);
            maneuverStart = 0;
        }
    }

    private void CheckManeuvers()
    {
        switch (maneuvers[0])
        {
            case "landToOrbit":
                landToOrbit.Run(flightParams, thrusterController);
                break;
            case "orbitToLand":
                orbitToLand.Run(GetFlightParams(), thrusterController);
                break;
            case "orbitToFlyBy":
                orbitToFlyBy.Run(GetFlightParams(), thrusterController);
                break;
            case "flyByToOrbit1":
                flyByToOrbit.Run(GetFlightParams(), thrusterController);
                break;
            case "flyByToOrbit2":
                flyByToOrbit.Run(GetFlightParams(), thrusterController);
                break;
        }
    }

    public void Miau()
    {
        ResetOrbit();
        Fly();

        flightParams = GetFlightParams();

        if (!(maneuvers.Length == 0))
        {
            CheckManeuvers();
            SetFlightParams(flightParams);
        }
    }

    public void SetManeuvers(string[] newManeuvers)
    {
        maneuvers = newManeuvers;
        maneuverStart = radiansTravelled;

        switch (maneuvers[0])
        {
            case "landToOrbit":
                SetLaunchOrbits();
                acceleration = Mathf.Abs(acceleration);
                break;
            case "orbitToLand":
                SetLandingOrbits();
                acceleration = -Mathf.Abs(acceleration);
                break;
            case "orbitToFlyBy":
                acceleration = Mathf.Abs(acceleration);
                SetFlyByOrbit();
                break;
            case "flyByToOrbit1":
                acceleration = -Mathf.Abs(acceleration);
                planetPosition = targetPlanetPosition;
                SetFlyByOrbit();
                break;
            case "flyByToOrbit2":
                acceleration = -Mathf.Abs(acceleration);
                planetPosition = currentPlanetPosition;
                SetFlyByOrbit();
                break;
        }
    }

    public void Fly()
    {
        switch (flightParams.FlyState)
        {
            case "ascending":
                Ascend();
                break;
            case "subOrbiting":
                SubOrbit();
                break;
            case "orbiting":
                Orbit();
                break;
            case "exiting":
                Exit();
                break;
            case "flyby":
                FlyBy();
                break;
            case "approach":
                Approach();
                break;
            case "descending":
                Descend();
                break;
            case "falling":
                Fall();
                break;
        }
    }

    private void Ascend()
    {
        speed += thrusterController.Thrust();
        float step = speed * Time.deltaTime;
        transform.position += bodyUp * step;
        Rotate();
    }

    private void Descend()
    {
        speed += acceleration * Time.deltaTime;
        float step = speed * Time.deltaTime;
        transform.position -= bodyUp * step;

        if (speed < 0)
        {
            speed = 0;
            transform.position = planetPosition + startPoint;
        }
        Rotate();
    }

    private void Fall()
    {
        speedVector -= (transform.position - planetPosition).normalized * 0.0005f;
        transform.position += speedVector * Time.deltaTime;

        if ((transform.position - planetPosition).magnitude < startPoint.magnitude) flyState = "crashed";
    }

    private void Turn(float targetTurning)
    {
        float turning = targetTurning - Mathf.PI + (radiansTravelled - maneuverStart) * (180 / 30);
        if (radiansTravelled - maneuverStart > (30 * Mathf.PI / 180)) turning = targetTurning;

        direction = turning;
    }

    private void SubOrbit()
    {
        speed += thrusterController.Thrust();
        radiansTravelled += speed / subOrbitRadius * Time.deltaTime;
        Vector3 subOrbitPosition = subOrbitRadius * (Mathf.Cos(radiansTravelled) * bodyForward + Mathf.Sin(radiansTravelled) * bodyUp);

        transform.position = subOrbitCenter + subOrbitPosition;
        Rotate();
    }

    private void Orbit()
    {
        radiansTravelled += speed / orbitRadius * Time.deltaTime;
        Vector3 orbitPosition = orbitRadius * (Mathf.Cos(radiansTravelled) * bodyForward + Mathf.Sin(radiansTravelled) * bodyUp);

        transform.position = planetPosition + orbitPosition;
        Rotate();
    }

    private void FlyBy()
    {
        Vector3 ellipseCenter = (targetPlanetPosition - currentPlanetPosition) / 2f + currentPlanetPosition;
        radiansTravelled += Mathf.Abs(speed / (transform.position - ellipseCenter).magnitude * Time.deltaTime);

        transform.position = ellipseCenter + (orbitRadius + 0.75f) * Mathf.Cos(radiansTravelled) * bodyForward + 1f * Mathf.Sin(radiansTravelled) * bodyUp;
        Rotate();
    }

    private void Rotate()
    {
        transform.rotation = Quaternion.Euler(0, -ToDegrees(radiansTravelled + direction), 90);
        speedVector = transform.up * speed;
    }

    private void Exit()
    {
        speed += acceleration * Time.deltaTime;
        FlyBy();
    }

    private void Approach()
    {
        speed += acceleration * Time.deltaTime;
        FlyBy();
    }

    private float ToDegrees(float angle)
    {
        float deg = angle * 180f / Mathf.PI;
        if (deg > 360) deg %= 360;

        return deg;
    }

    private float roundRadians(float rad)
    {
        return rad %= 2 * Mathf.PI;
    }
}
