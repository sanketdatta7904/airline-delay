using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class clickEvent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // mouse click listener
    private void OnMouseDown()
    {
        Debug.Log("Mouse down");
        // mouse position
        Vector3 mousePos = Input.mousePosition;
        // convert mouse position to world position
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);


        // get the closest object
        PointScript nearestObj = MarkSpawner.getClosestPoint(worldPos);
        // get the distamce between the two points
        float distance = Vector3.Distance(nearestObj.transform.position, worldPos);
        string message = "Airport Name: " + nearestObj.airportName + "\n" +
                         "Airport Code: " + nearestObj.airportCode + "\n" +
                         "Average Delay: " + nearestObj.avgDelay + "\n" +
                         "Distance: " + distance + "\n" +
                         "type: " + nearestObj.airportType;
        Debug.Log(distance);

        if(distance < 1.0005)
            TooltipManager._instance.SetAndShowTooltip(message);
        else
            TooltipManager._instance.HideTooltip();
    }

    //mouse move listener
    //private void OnMouseOver()
    //{
    //    Debug.Log("Mouse over");

    //}

}
