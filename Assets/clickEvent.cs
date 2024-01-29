using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class clickEvent : MonoBehaviour
{
    // Reference to the BarChartScript
    public BarChartScript barChart;

    // ...

    private void OnMouseDown()
    {
        Debug.Log("Mouse down");
        Vector3 mousePos = Input.mousePosition;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

        // Get the closest object
        PointScript nearestObj = MarkSpawner.getClosestPoint(worldPos);
        float distance = Vector3.Distance(nearestObj.transform.position, worldPos);
        string message = "Airport Name: " + nearestObj.airportName + "\n" +
                         "Airport Code: " + nearestObj.airportCode + "\n" +
                         "Average Delay: " + nearestObj.avgDelay + "\n" +
                         "Distance: " + distance + "\n" +
                         "Type: " + nearestObj.airportType;
        Debug.Log(distance);

        if (distance < 1.0005)
        {
            // Set and show tooltip
            TooltipManager._instance.SetAndShowTooltip(message);

            // Update the bar chart with the average delay data of the selected airport
            float[] newData = { (float)nearestObj.avgDelay }; // Explicit cast from double to float
            barChart.UpdateData(newData);
        }
        else
        {
            // Hide tooltip
            TooltipManager._instance.HideTooltip();
        }
    }
}
