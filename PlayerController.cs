using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject rocket;
    public GameObject thruster;
    public GameObject thruster2;
    public GameObject capsule;

    RocketController rocketController;

    Bluetooth bluetooth = new Bluetooth();

    public GameObject nextBtn;
    public GameObject prevBtn;
    public GameObject stageBtn;

    ButtonHandler nextBtnHandler;
    ButtonHandler prevBtnHandler;
    ButtonHandler stageBtnHandler;

    List<RocketController> controllers;

    void Start()
    {
        //bluetooth.Start();

        rocket.AddComponent<RocketController>();
        rocketController = rocket.GetComponent<RocketController>();

        controllers = new List<RocketController>();
        controllers.Add(rocketController);

        rocketController.AddCapsule(capsule);
        rocketController.AddThruster(thruster);
        rocketController.AddThruster(thruster2);

        rocketController.SetDirections();

        nextBtnHandler = nextBtn.GetComponent<ButtonHandler>();
        prevBtnHandler = prevBtn.GetComponent<ButtonHandler>();
        stageBtnHandler = stageBtn.GetComponent<ButtonHandler>();
    }

    private void Update()
    {
        //bluetooth.Update();
        //bluetooth.BleState();

        if (rocketController.maneuvers.Length == 0) CheckOptions();

        for (int i = controllers.Count - 1; i >= 0; i--)
        {
            controllers[i].Miau();
            if (controllers[i].flyState == "crashed")
            {
                Destroy(controllers[i].gameObject);
                controllers.RemoveAt(i);
            }
        }

        if (rocketController.flyState != "landed") stageBtn.SetActive(true);
    }

    private void CheckOptions()
    {
        nextBtn.SetActive(true);
        prevBtn.SetActive(true);
        stageBtn.SetActive(false);

        switch (rocketController.flyState)
        {
            case "landed":
                nextBtnHandler.SetManeuvers(new string[] { "landToOrbit" });
                nextBtnHandler.SetLabel("Orbit");

                prevBtn.SetActive(false);
                break;
            case "orbiting":
                nextBtnHandler.SetManeuvers(new string[] { "orbitToFlyBy" });
                nextBtnHandler.SetLabel("Fly by");

                prevBtnHandler.SetManeuvers(new string[] { "orbitToLand" });
                prevBtnHandler.SetLabel("Land");
                break;
            case "flyby":
                nextBtnHandler.SetManeuvers(new string[] { "flyByToOrbit1" });
                nextBtnHandler.SetLabel("Orbit Mars");

                prevBtnHandler.SetManeuvers(new string[] { "flyByToOrbit2" });
                prevBtnHandler.SetLabel("Orbit Earth");
                break;
        }
    }

    public void Stage()
    {
        GameObject dettached = rocketController.Dettach();
        GameObject rocket2 = new GameObject("Rocket2");
        rocket2.AddComponent<RocketController>();
        RocketController thrusterController = rocket2.GetComponent<RocketController>();
        controllers.Add(thrusterController);

        thrusterController.AddThruster(dettached);
        thrusterController.DisableBooster();
        FlightParams flightParams = rocketController.GetFlightParams();
        thrusterController.SetFlightParams(flightParams);
        thrusterController.flyState = "falling";
    }

    public void SetManeuvers(string[] newManeuvers)
    {
        rocketController.SetManeuvers(newManeuvers);
        nextBtn.SetActive(false);
        prevBtn.SetActive(false);
    }
}
