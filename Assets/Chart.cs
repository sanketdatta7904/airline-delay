using UnityEngine;

public class Chart : MonoBehaviour
{
    public Transform pointPrefab;
    public Vector3[] dataPoints;

    void Start()
    {
        if (dataPoints.Length < 2)
        {
            Debug.LogError("Not enough data points to draw a line chart.");
            return;
        }

        // Draw the line chart
        DrawLineChart();
    }

    void DrawLineChart()
    {
        LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = dataPoints.Length;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        for (int i = 0; i < dataPoints.Length; i++)
        {
            Instantiate(pointPrefab, dataPoints[i], Quaternion.identity);
            lineRenderer.SetPosition(i, dataPoints[i]);
        }
    }
}
