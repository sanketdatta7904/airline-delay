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



        DrawAxisWithLabels();
    }

    
    
 void DrawAxisWithLabels()
    {
        DrawAxis(); // Draw the axes

        // Add arrowhead at the specific point (X = 0, Y = 146)
        Vector2 specificPoint = new Vector2(0f, 146f);
        // AddArrowhead(xAxis, specificPoint, Quaternion.Euler(0f, 0f, 0f));

        // Add vertical text to the Y-axis
        AddTextToAxis(yAxis, "minutes", TextAnchor.UpperCenter);
    }

    void AddTextToAxis(RectTransform axis, string labelText, TextAnchor alignment)
{
    GameObject textObject = new GameObject("AxisLabel");
    RectTransform textRect = textObject.AddComponent<RectTransform>();
    Text textComponent = textObject.AddComponent<Text>();

    textComponent.text = labelText;

    // Load the font from the system fonts
    Font defaultFont = Font.CreateDynamicFontFromOSFont("Arial", 12); // You may need to adjust the font size
    if (defaultFont != null)
    {
        textComponent.font = defaultFont;
    }
    else
    {
        Debug.LogError("Failed to load default system font.");
        Destroy(textObject);
        return;
    }

    textComponent.fontSize = 12;
    textComponent.alignment = alignment;

    // Set the color of the text to black
    textComponent.color = Color.black;

    // Adjust the position calculation for the text
    textRect.anchoredPosition = new Vector2(30f, axisLengthY / 2f); // Shift by 30 pixels to the right

    if (axis != null && axis.parent != null)
    {
        textRect.SetParent(axis.parent, false);
    }
    else
    {
        Debug.LogError("Axis is null or missing parent.");
        Destroy(textObject);
        return;
    }

    // Set the rotation to make the text vertical
    textRect.localRotation = Quaternion.Euler(0f, 0f, 90f);
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
