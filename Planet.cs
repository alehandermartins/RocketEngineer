using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The Planet class is responsible for generating a tiny procedural planet. It does this by subdividing an Icosahedron, then
// randomly selecting groups of Polygons to extrude outwards. These become the lowlands and hills of the planet, while the
// unextruded Polygons become the ocean.

public class Planet : MonoBehaviour
{
    // These public parameters can be tweaked to give different styles to your planet.

    public Material m_GroundMaterial;
    public Material m_OceanMaterial;

    PlanetGenerator planet;
    public GameObject tree;
    public GameObject rock;
    public GameObject rocket;

    public bool withRocket = false;

    public void Start()
    {
        planet = new PlanetGenerator(gameObject, m_GroundMaterial, m_OceanMaterial, withRocket);
        GameObject element = tree;

        if (!withRocket)
        {
            element = rock;
        }
        for (int i = 0; i < 20; i++)
        {
            GameObject ele = Instantiate(element, Vector3.zero, Quaternion.identity);
            planet.AddComponent(ele, 0.0025f, 0);
        }

       // if (withRocket)
        //{
            //planet.AddComponent(rocket, 0.003f, 0.015f);
            //RocketController rocketController = rocket.GetComponent<RocketController>();
            //rocketController.SetDirections();
        //}
    }
}