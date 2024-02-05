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

        // Add arrowhead at the specific point (X = 0, Y = 146)
        Vector2 specificPoint = new Vector2(0f, 146f);
        // AddArrowhead(xAxis, specificPoint, Quaternion.Euler(0f, 0f, 0f));
    }
void DrawAxis()
{
    // Draw the positive side of the X and Y axes
    DrawLine(xAxis, new Vector2(0f, 0f), new Vector2(axisLengthX, 0f), axisThickness, true);
    DrawLine(yAxis, new Vector2(0f, 0f), new Vector2(0f, axisLengthY), axisThickness, true);

    // Add arrowhead to the end of the X-axis
    AddArrowhead(xAxis, new Vector2(axisLengthX, 0f), Quaternion.Euler(0f, 0f, 0f));
    
    // Add arrowhead to the end of the Y-axis
    AddArrowhead(yAxis, new Vector2(0f, axisLengthY), Quaternion.Euler(0f, 0f, 90f));
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
            AddArrowhead(line, endPos, line.localRotation);
        }
    }

void AddArrowhead(RectTransform line, Vector2 position, Quaternion rotation)
{
    GameObject arrowhead = new GameObject("Arrowhead");
    RectTransform arrowheadRect = arrowhead.AddComponent<RectTransform>();
    Image arrowheadImage = arrowhead.AddComponent<Image>();

    string path = "Assets/arrow.png"; // Adjust the path as per the actual location
    Sprite arrowheadSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);

    if (arrowheadSprite != null)
    {
        arrowheadImage.sprite = arrowheadSprite;
        arrowheadRect.sizeDelta = new Vector2(arrowheadSize, arrowheadSize);

        // Adjust the position calculation
        arrowheadRect.anchoredPosition = position;

        if (line != null && line.parent != null)
        {
            arrowheadRect.SetParent(line.parent, false);
        }
        else
        {
            Debug.LogError("Line is null or missing parent.");
            Destroy(arrowhead);
            return;
        }

        arrowheadRect.anchoredPosition = position;
        arrowheadRect.localRotation = rotation;
    }
    else
    {
        Debug.LogError("Failed to load arrowhead sprite.");
        Destroy(arrowhead);
    }
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
