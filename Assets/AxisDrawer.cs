using UnityEngine;
using UnityEngine.UI;

public class AxisDrawer : MonoBehaviour
{
    public RectTransform xAxis;
    public RectTransform yAxis;
    public float axisLengthX = 200f; // Set the desired length for X-axis
    public float axisLengthY = 135f; // Set the desired length for Y-axis
    public float axisThickness = 1.0f; // Set the desired thickness for axes
    public float arrowheadSize = 5.0f; // Set the size of the arrowheads

    void Start()
    {
        // Manually set references if not assigned in the Inspector
        if (xAxis == null)
        {
            xAxis = FindAxisByName("XAxis");
        }

        if (yAxis == null)
        {
            yAxis = FindAxisByName("YAxis");
        }

        DrawAxis();
    }

    void DrawAxis()
    {
        // Draw the positive side of the X and Y axes
        DrawLine(xAxis, new Vector2(0f, 0f), new Vector2(axisLengthX, 0f), axisThickness, true);
        DrawLine(yAxis, new Vector2(0f, 0f), new Vector2(0f, axisLengthY), axisThickness, true);
    }

 void DrawLine(RectTransform line, Vector2 startPos, Vector2 endPos, float thickness, bool withArrowhead = false)
{
    Image lineImage = line.GetComponent<Image>();
    if (lineImage == null)
    {
        lineImage = line.gameObject.AddComponent<Image>();
        lineImage.color = Color.grey; // Set the color of the axis lines to grey
    }
    else
    {
        lineImage.color = Color.grey; // Update the color if the Image component already exists
    }

    line.sizeDelta = new Vector2(Vector2.Distance(startPos, endPos), thickness);
    line.anchoredPosition = (startPos + endPos) / 2f;
    line.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(endPos.y - startPos.y, endPos.x - startPos.x) * Mathf.Rad2Deg);

    if (withArrowhead)
    {
        DrawArrowhead(line, endPos, line.localRotation);
    }
}
void DrawArrowhead(RectTransform line, Vector2 position, Quaternion rotation)
{
    GameObject arrowhead = new GameObject("Arrowhead");
    RectTransform arrowheadRect = arrowhead.AddComponent<RectTransform>();
    Image arrowheadImage = arrowhead.AddComponent<Image>();

    arrowheadImage.color = Color.grey; // Set the color of the arrowhead to grey

    arrowheadRect.sizeDelta = new Vector2(arrowheadSize, arrowheadSize);
    arrowheadRect.anchoredPosition = position;
    arrowheadRect.localRotation = rotation * Quaternion.Euler(0f, 0f, -90f); // Correct rotation for a triangle

    // Check if line is not null and has a valid parent
    if (line != null && line.parent != null)
    {
        arrowheadRect.SetParent(line.parent, false);
    }
    else
    {
        Debug.LogError("Line is null or missing parent.");
        Destroy(arrowhead); // Clean up the arrowhead if there's an issue
        return;
    }

    // Create an arrowhead using LineRenderer
    LineRenderer arrowLine = arrowhead.AddComponent<LineRenderer>();
    arrowLine.material = new Material(Shader.Find("Standard")); // You might need to adjust the material based on your requirements
    arrowLine.startWidth = arrowLine.endWidth = axisThickness; // Use axisThickness instead of lineWidth

    // Set arrowhead positions
    Vector3 p0 = new Vector3(0, arrowheadSize / 2, 0);
    Vector3 p1 = new Vector3(0, -arrowheadSize / 2, 0);

    // Convert local positions to world positions
    p0 = arrowheadRect.TransformPoint(p0);
    p1 = arrowheadRect.TransformPoint(p1);

    arrowLine.positionCount = 2;
    arrowLine.SetPosition(0, arrowheadRect.position);
    arrowLine.SetPosition(1, p0);
    arrowLine.useWorldSpace = true; // Set to use world space coordinates
}




    RectTransform FindAxisByName(string axisName)
    {
        GameObject axisObject = GameObject.Find(axisName);
        if (axisObject != null)
        {
            RectTransform axisRectTransform = axisObject.GetComponent<RectTransform>();
            if (axisRectTransform != null)
            {
                return axisRectTransform;
            }
        }

        Debug.LogError($"Failed to find or retrieve RectTransform for {axisName}.");
        return null;
    }
}
