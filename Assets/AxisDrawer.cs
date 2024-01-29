using UnityEngine;
using UnityEngine.UI;

public class AxisDrawer : MonoBehaviour
{
    public RectTransform xAxis;
    public RectTransform yAxis;
    public float axisLengthX = 300f; // Set the desired length for X-axis
    public float axisLengthY = 135f; // Set the desired length for Y-axis
    public float axisThickness = 2f; // Set the desired thickness for axes

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
        DrawLine(xAxis, new Vector2(0f, 0f), new Vector2(axisLengthX, 0f), axisThickness);
        DrawLine(yAxis, new Vector2(0f, 0f), new Vector2(0f, axisLengthY), axisThickness);
    }

    void DrawLine(RectTransform line, Vector2 startPos, Vector2 endPos, float thickness)
    {
        Image lineImage = line.GetComponent<Image>();
        if (lineImage == null)
        {
            lineImage = line.gameObject.AddComponent<Image>();
            lineImage.color = Color.black; // Set the color of the axis lines
        }

        line.sizeDelta = new Vector2(Vector2.Distance(startPos, endPos), thickness);
        line.anchoredPosition = (startPos + endPos) / 2f;
        line.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(endPos.y - startPos.y, endPos.x - startPos.x) * Mathf.Rad2Deg);
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
