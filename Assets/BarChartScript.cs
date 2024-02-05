using UnityEngine;
using UnityEngine.UI;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    using System.Data.SQLite;
#endif
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
    using Mono.Data.Sqlite;
#endif

public class BarChartScript : MonoBehaviour
{
    public RectTransform barPrefab;
    public RectTransform chartContainer;
    public float barWidth = 10f;
    public float spacing = 1f;
    private float[] data; 
     public Text chartTitle;


     private string[] airportNames;

       public Text chartSubtitle; // 
      public AxisDrawer axisDrawer; 


public float axisLengthY = 135f;

void Start()
{
    // Fetch avg_delay data from the SQL table and assign it to the data array
    FetchAvgDelayData();
    CreateBars();
}



void FetchAvgDelayData()
{
    string dbPath = "URI=file:" + Application.dataPath + "/../../aviation.db";
    IDbConnection dbConnection = null;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    dbConnection = new System.Data.SQLite.SQLiteConnection(dbPath);
#endif
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
    dbConnection = new Mono.Data.Sqlite.SqliteConnection(dbPath);
#endif

    try
    {
        dbConnection.Open();
        string query = "SELECT airport_name, avg_delay FROM aggregated_delays ORDER BY avg_delay DESC LIMIT 5";
        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = query;

        IDataReader reader = dbCommand.ExecuteReader();

        // Initialize the data and airportNames arrays with the absolute value of avg_delay and airport_name values from the SQL table
        List<float> dataList = new List<float>();
        List<string> airportNamesList = new List<string>();

        while (reader.Read())
        {
            object avgDelayObject = reader["avg_delay"];
            float avgDelay = (avgDelayObject != DBNull.Value) ? Convert.ToSingle(avgDelayObject) : 0f;
            dataList.Add(Mathf.Abs(avgDelay)); // Take the absolute value

            object airportNameObject = reader["airport_name"];
            airportNamesList.Add((airportNameObject != DBNull.Value) ? Convert.ToString(airportNameObject) : "");
        }

        data = dataList.ToArray();
        airportNames = airportNamesList.ToArray();
    }
    catch (Exception e)
    {
        Debug.LogError($"Error executing query: {e.Message}");
    }
    finally
    {
        if (dbConnection != null && dbConnection.State == ConnectionState.Open)
            dbConnection.Close();
    }
}





void CreateBars()
{
    // Clear existing bars
    foreach (Transform child in chartContainer)
    {
        Destroy(child.gameObject);
    }

    if (barPrefab == null || chartContainer == null || axisDrawer == null)
    {
        Debug.LogError("Assign barPrefab, chartContainer, and axisDrawer");
        return;
    }

    float axisLengthY = axisDrawer.axisLengthY;

    float totalWidth = (barWidth + spacing) * (data.Length + 2) - spacing;
    float startX = -totalWidth / 2f;

    // Calculate the total height of the bars
    float totalHeight = data.Max();

    // Adjust yPos for all bars to align bottoms to Y = 0
    for (int i = 0; i < data.Length; i++)
    {
        float xPos = startX + i * (barWidth + spacing);
        float yPos = -axisLengthY / 2f + Mathf.Abs(data[i]) / 2f; // Use absolute value to align bottoms to Y = 0

        RectTransform barInstance = Instantiate(barPrefab, chartContainer);
        barInstance.sizeDelta = new Vector2(barWidth, 0f);
        barInstance.anchoredPosition = new Vector2(xPos, yPos);

        // Set the color of the bar based on the delay value
        Image barImage = barInstance.GetComponent<Image>();
        if (barImage != null)
        {
            barImage.color = (data[i] < 0) ? Color.red : Color.blue;
        }

        // Create and set the text label under each bar
        GameObject textLabel = new GameObject("BarLabel", typeof(RectTransform));
        textLabel.transform.SetParent(chartContainer);
        textLabel.transform.localScale = Vector3.one;
        textLabel.transform.localPosition = new Vector2(xPos + barWidth / 2f - 13f, yPos - 20f); // Adjusted yPos for the label

        Text labelText = textLabel.AddComponent<Text>();
        labelText.text = Mathf.RoundToInt(Mathf.Abs(data[i])).ToString(); // Use absolute value for the label text
        labelText.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.color = Color.white;

        // Set the sibling index of the text label higher than the bar to ensure it renders above the bar
        textLabel.transform.SetSiblingIndex(barInstance.transform.GetSiblingIndex() + 1);

        // LeanTween to smoothly increase the height of the bars
        LeanTween.value(barInstance.gameObject, 0f, Mathf.Abs(data[i]), 1f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnUpdate((float value) =>
            {
                barInstance.sizeDelta = new Vector2(barWidth, value);
            });
    }
}






    public void UpdateChartTitle(string title)
    {
        if (chartTitle != null)
        {
            chartTitle.text = title;
                       chartTitle.fontStyle = FontStyle.Bold;
        }
    }

    public void UpdateSubtitle(string subtitle)
    {
        if (chartSubtitle != null)
        {
            chartSubtitle.text = subtitle;
              chartSubtitle.fontSize = 12;
              
        }
    }

public void UpdateData(float[] newData)
{
    data = newData;
    CreateBars();
}

// public void HideSubtitle()
// {
//     if (chartSubtitle != null)
//     {
//         chartSubtitle.gameObject.SetActive(false);
//     }
// }

public void HideSubtitle()
{
    if (chartSubtitle != null)
    {
        chartSubtitle.gameObject.SetActive(false);
    }
}

public void ShowSubtitle()
{
    if (chartSubtitle != null)
    {
        chartSubtitle.gameObject.SetActive(true);
    }
}

}