using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class clickEventLine : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnMouseDown()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        worldPos.z = 0; // Assuming your game objects are at z = 0

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

            if (lineDistance < 1.0f) // Adjust this threshold as needed
            {
                HandleLineClick(nearestLine, lineDistance);
            }
        }
    }

    private void HandleLineClick(GameObject lineObject, float lineDistance)
    {
        // Logic for handling line click
        LineScript lineScript = lineObject.GetComponent<LineScript>();
        if (lineScript != null)
        {
            string message ="Route ID: " + lineScript.routeID + "\n" +
                            "Average Delay: " + lineScript.averageDelay + "\n" +
                            "Traffic Count: " + lineScript.trafficCount + "\n" +
                            "Distance: " + lineDistance + "\n" +
                            "Source Airport: " + lineScript.sourceAirport + "\n" +
                            "Source Country: " + lineScript.sourceCountry + "\n" +
                            "Dest Airport: " + lineScript.destAirport + "\n" +
                            "Dest Country: " + lineScript.destCountry;

            TooltipManager._instance.SetAndShowTooltip(message);
        }
    }
    private GameObject FindClosestLine(Vector3 worldPos)
    {
        GameObject closestLine = null;
        float closestDistance = float.MaxValue;

        GameObject[] lineObjects = GameObject.FindGameObjectsWithTag("Line");
        Debug.Log("Number of line objects found: " + lineObjects.Length);

        foreach (GameObject lineObject in lineObjects)
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

        if (closestLine != null)
        {
            Debug.Log("Closest line found: " + closestLine.name);
        }
        else
        {
            Debug.Log("No closest line found");
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


}
