using UnityEngine;
using UnityEngine.UI;

public class AxisDrawer : MonoBehaviour
{
    public RectTransform xAxis;
    public RectTransform yAxis;
    public float axisLengthX = 200f;
    public float axisLengthY = 120f;
    public float axisThickness = 1.0f;
    public float arrowheadSize = 5.0f;

    void Start()
    {
        if (xAxis == null)
        {
            xAxis = FindAxisByName("XAxis");
        }

        if (yAxis == null)
        {
            yAxis = FindAxisByName("YAxis");
        }

        DrawAxisWithLabels();
    }

    void DrawAxisWithLabels()
    {
        DrawAxis();

        // Change label to "minutes" with regular font size
        AddTextToAxis(yAxis, "minutes", TextAnchor.UpperCenter, new Vector2(15f, axisLengthY / 2f), 12);

        // Add dashes on Y-axis starting from 20 and incrementing by 20
        for (int i = 20; i <= 100; i += 20)
        {
            AddDashToAxis(yAxis, new Vector2(30f, i));
            AddTextToAxis(yAxis, i.ToString(), TextAnchor.LowerLeft, new Vector2(-57f, i + 44f), 9); // Adjust font size here
        }
    }

    void DrawAxis()
    {
        DrawLine(xAxis, new Vector2(0f, 0f), new Vector2(axisLengthX, 0f), axisThickness, true);
        DrawLine(yAxis, new Vector2(0f, 0f), new Vector2(0f, axisLengthY), axisThickness, true);

        AddArrowhead(xAxis, new Vector2(axisLengthX, 0f), Quaternion.Euler(0f, 0f, 0f));
        AddArrowhead(yAxis, new Vector2(0f, axisLengthY), Quaternion.Euler(0f, 0f, 90f));
    }

    void DrawLine(RectTransform line, Vector2 startPos, Vector2 endPos, float thickness, bool withArrowhead = false)
    {
        Image lineImage = line.GetComponent<Image>();
        if (lineImage == null)
        {
            lineImage = line.gameObject.AddComponent<Image>();
            lineImage.color = Color.grey;
        }
        else
        {
            lineImage.color = Color.grey;
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

        string path = "Assets/arrow.png";
        Sprite arrowheadSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);

        if (arrowheadSprite != null)
        {
            arrowheadImage.sprite = arrowheadSprite;
            arrowheadRect.sizeDelta = new Vector2(arrowheadSize, arrowheadSize);

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

    void AddDashToAxis(RectTransform axis, Vector2 position)
    {
        GameObject dashObject = new GameObject("Dash");
        RectTransform dashRect = dashObject.AddComponent<RectTransform>();
        Image dashImage = dashObject.AddComponent<Image>();

        dashImage.color = Color.grey;
        dashRect.sizeDelta = new Vector2(5f, 1f); // Adjust the size of the dash

        dashRect.anchoredPosition = new Vector2(position.x - 33f, position.y); // Adjust the X-coordinate

        if (axis != null && axis.parent != null)
        {
            dashRect.SetParent(axis.parent, false);
        }
        else
        {
            Debug.LogError("Axis is null or missing parent.");
            Destroy(dashObject);
            return;
        }
    }

    void AddTextToAxis(RectTransform axis, string labelText, TextAnchor alignment, Vector2 position, int fontSize)
    {
        GameObject textObject = new GameObject("AxisLabel");
        RectTransform textRect = textObject.AddComponent<RectTransform>();
        Text textComponent = textObject.AddComponent<Text>();

        textComponent.text = labelText;

        // Load the font from the system fonts
        Font defaultFont = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
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

        textComponent.fontSize = fontSize;
        textComponent.alignment = alignment;
        textComponent.color = Color.black;

        // Set the position of the text
        textRect.anchoredPosition = position;

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
