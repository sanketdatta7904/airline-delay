using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class clickEventRoute : MonoBehaviour
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
    // private void OnMouseDown()
    // {
    //     Debug.Log("Mouse down");
    //     // mouse position
    //     Vector3 mousePos = Input.mousePosition;
    //     // convert mouse position to world position
    //     Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);


    //     // get the closest object
    //     PointScript nearestObj = MarkSpawner.getClosestPoint(worldPos);
    //     // get the distamce between the two points
    // float distance = Vector3.Distance(nearestObj.transform.position, worldPos);
    // string message = "Airport Name: " + nearestObj.airportName + "\n" +
    //                  "Airport Code: " + nearestObj.airportCode + "\n" +
    //                  "Average Delay: " + nearestObj.avgDelay + "\n" +
    //                  "Distance: " + distance;
    //     Debug.Log(distance);

    //     if(distance < 1.0005)
    //         TooltipManager._instance.SetAndShowTooltip(message);
    //     else
    //         TooltipManager._instance.HideTooltip();
    // }
    private void OnMouseDown()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        worldPos.z = 0; // Assuming your game objects are at z = 0

        // Find the closest airport
        PointScript nearestAirport = MarkSpawner1.getClosestPoint(worldPos);
        float airportDistance = nearestAirport != null ? Vector3.Distance(worldPos, nearestAirport.transform.position) : float.MaxValue;

        // Find the closest line
        GameObject nearestLine = FindClosestLine(worldPos);
        float lineDistance = float.MaxValue;
        if (nearestLine != null)
        {
            LineRenderer lineRenderer = nearestLine.GetComponent<LineRenderer>();
            if (lineRenderer != null)
            {
                for (int i = 0; i < lineRenderer.positionCount - 1; i++)
                {
                    Vector3 start = lineRenderer.GetPosition(i);
                    Vector3 end = lineRenderer.GetPosition(i + 1);
                    float distance = CalculateDistanceToLine(worldPos, start, end);
                    if (distance < lineDistance)
                    {
                        lineDistance = distance;
                    }
                }
            }
        }

        // Determine if the closest object is an airport or a line
        if (airportDistance < lineDistance)
        {
            HandleAirportClick(nearestAirport.gameObject, airportDistance);
        }
        else if (nearestLine != null)
        {
            HandleLineClick(nearestLine, lineDistance);
        }
    }

    private void HandleAirportClick(GameObject airportObject, float airportDistance)
    {
        // Existing logic for handling airport click
        PointScript nearestObj = airportObject.GetComponent<PointScript>();
        if (nearestObj != null)
        {
            string message = "Airport Name: " + nearestObj.airportName + "\n" +
                             "Airport Code: " + nearestObj.airportCode + "\n" +
                             "Average Delay: " + nearestObj.avgDelay + "\n" +
                             "Distance: " + airportDistance;
            TooltipManager._instance.SetAndShowTooltip(message);
        }
    }

    private void HandleLineClick(GameObject lineObject, float lineDistance)
    {
        // Logic for handling line click
        // Assuming you have a script attached to the line that stores routeID, averageDelay, and trafficCount
        LineScript lineScript = lineObject.GetComponent<LineScript>();
        if (lineScript != null)
        {
            string message = "Route ID: " + lineScript.routeID + "\n" +
                             "Average Delay: " + lineScript.averageDelay + "\n" +
                             "Traffic Count: " + lineScript.trafficCount + "\n" +
                             "Distance: " + lineDistance; ;
            TooltipManager._instance.SetAndShowTooltip(message);
        }
    }
    private GameObject FindClosestLine(Vector3 worldPos)
    {
        GameObject closestLine = null;
        float closestDistance = float.MaxValue;

        foreach (GameObject lineObject in GameObject.FindGameObjectsWithTag("Line"))
        {
            LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
            if (lineRenderer != null)
            {
                for (int i = 0; i < lineRenderer.positionCount - 1; i++)
                {
                    Vector3 start = lineRenderer.GetPosition(i);
                    Vector3 end = lineRenderer.GetPosition(i + 1);
                    float distance = CalculateDistanceToLine(worldPos, start, end);
                    if (distance < closestDistance)
                    {
                        closestLine = lineObject;
                        closestDistance = distance;
                    }
                }
            }
        }

        return closestLine;
    }

    private float CalculateDistanceToLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        // Get vector from point to start
        Vector3 lineVec = lineEnd - lineStart;
        Vector3 pointVec = point - lineStart;

        // Project pointVec onto lineVec
        float t = Vector3.Dot(pointVec, lineVec) / lineVec.sqrMagnitude;
        t = Mathf.Clamp01(t); // Clamp between 0 and 1 to stay within the line segment

        // Find the closest point in the line segment to the point
        Vector3 nearest = lineStart + t * lineVec;

        // Return the distance from the point to the line segment
        return Vector3.Distance(nearest, point);
    }

    //mouse move listener
    //private void OnMouseOver()
    //{
    //    Debug.Log("Mouse over");

    //}

}
