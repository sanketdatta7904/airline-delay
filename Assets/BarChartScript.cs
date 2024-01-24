using UnityEngine;
using UnityEngine.UI;

public class BarChartScript : MonoBehaviour
{
    public RectTransform barPrefab;
    public RectTransform chartContainer;
    public float barWidth = 20f;
    public float spacing = 10f;
    public float[] data;

    void Start()
    {
        data = new float[] { 30f, 50f, 20f, 10f };
        CreateBars();
    }

    void CreateBars()
    {
        if (barPrefab == null || chartContainer == null)
        {
            Debug.LogError("Please assign the barPrefab and chartContainer in the inspector.");
            return;
        }

        float totalWidth = (barWidth + spacing) * (data.Length + 2) - spacing; // Add 2 for the additional bars
        float startX = -totalWidth / 2f;

        for (int i = 0; i < data.Length + 2; i++)
        {
            float xPos = startX + i * (barWidth + spacing);
            float yPos = i < data.Length ? data[i] / 2f : 0;

            RectTransform barInstance = Instantiate(barPrefab, chartContainer);
            barInstance.sizeDelta = new Vector2(barWidth, Mathf.Abs(yPos) * 2);
            barInstance.anchoredPosition = new Vector2(xPos, yPos);

            // Optional: Customize the appearance of the bar (color, etc.)
            Image barImage = barInstance.GetComponent<Image>();
            if (barImage != null)
            {
                // Set the color or other properties of the bar
                // Example: barImage.color = Color.blue;
            }
        }
    }
}