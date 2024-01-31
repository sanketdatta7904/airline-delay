using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class clickEvent : MonoBehaviour
{
    void OnMouseDown()
    {
        Debug.Log("Mouse down");
        Vector3 mousePos = Input.mousePosition;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

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
            // Show tooltip
            TooltipManager._instance.SetAndShowTooltip(message);

            // Update chart data
            UpdateChart(nearestObj.avgDelay);
        }
        else
        {
            // Hide tooltip
            TooltipManager._instance.HideTooltip();
        }
    }

void UpdateChart(double avgDelay)
{
    // Access the BarChartScript and update its data
    BarChartScript barChart = FindObjectOfType<BarChartScript>();
    if (barChart != null)
    {
        // Convert avgDelay to float and update chart with the selected airport's average delay
        float[] newData = new float[] { (float)avgDelay /* add other data points as needed */ };
        barChart.UpdateData(newData);
    }
}

}
