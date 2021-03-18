using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ButtonHandler : MonoBehaviour
{
    private string[] maneuvers;

    public GameObject canvas;
    private PlayerController playerController;
    public GameObject text;

    void Start()
    {
        playerController = canvas.GetComponent<PlayerController>();
    }

    public void SetManeuvers(string[] newManeuvers)
    {
        maneuvers = newManeuvers;
    }

    public void PassManeuvers()
    {
        playerController.SetManeuvers(maneuvers);
    }

    public void SetLabel(string label)
    {
        text.GetComponent<Text>().text = label;
    }

    public void Stage()
    {
        playerController.Stage();
    }
}
