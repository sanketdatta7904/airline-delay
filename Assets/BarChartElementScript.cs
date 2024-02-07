using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// this script is attached to the BarChartElement prefab
// it stores all the necessary information about the airport represented by the bar
public class BarChartElementScript : MonoBehaviour
{
    public string airportName;
    public string airportCode;
    public int year;
    public float avgDelay;
    public Vector3 topLeftCorner;
    public Vector3 bottomRightCorner;
    public bool isShowingTopFive;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
