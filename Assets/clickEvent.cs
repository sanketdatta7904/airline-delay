using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using System.Data.SQLite;
#endif

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
using Mono.Data.Sqlite;
#endif

public class clickEvent : MonoBehaviour
{
    public GameObject SearchPanel;


void OnMouseDown()
{
    BarChartScript barChart = FindObjectOfType<BarChartScript>();  // Declare barChart outside the if statement

    Debug.Log("Mouse down at: " + Input.mousePosition);
    Vector3 mousePos = Input.mousePosition;
    Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

    // we dont want to allow the user to click on the search panel
    var widthSearchPanel = SearchPanel.GetComponent<RectTransform>().rect.width;
    var scaleSearchPanel = SearchPanel.transform.localScale.x;

    if (mousePos.x < widthSearchPanel * scaleSearchPanel)
    {
        Debug.Log("Clicked on the search panel");
        // Hide tooltip
        TooltipManager._instance.HideTooltip();
            // get closest bar
            BarChartElementScript nearestBar = BarChartScript.getClosestBar(mousePos);
            float distanceBar = Vector3.Distance(nearestBar.transform.position, worldPos);
            float avgDelayRounded = (float)Math.Round(nearestBar.avgDelay, 2);
            string TooltipMessage = "Airport Name: " + nearestBar.airportName + "\n" +
                                    "Airport Code: " + nearestBar.airportCode + "\n" +
                                    "Year: " + nearestBar.year + "\n" +
                                    "Average Delay: " + avgDelayRounded + "\n" +
                                    "Distance: " + distanceBar;
            TooltipManager._instance.SetAndShowTooltip(TooltipMessage);

        return;

    }

    PointScript nearestObj = MarkSpawner.getClosestPoint(worldPos);

    float distance = Vector3.Distance(nearestObj.transform.position, worldPos);

    string message = "Airport Name: " + nearestObj.airportName + "\n" +
                     "Airport Code: " + nearestObj.airportCode + "\n" +
                     "Average Delay: " + nearestObj.avgDelay + "\n" +
                     //     "Distance: " + distance + "\n" +
                     "Type: " + nearestObj.airportType;

    Debug.Log(distance);


    if (distance < 1.0005)
    {
        // Show tooltip
        TooltipManager._instance.SetAndShowTooltip(message);

        // Update chart data for the selected mark
        UpdateChart(nearestObj, barChart);

        // Show subtitle
        barChart.ShowSubtitle();
        barChart.AddLabelsUnderBars(new string[]{"2017", "2018", "2019", "2020", "2021"});
    }
    else
    {
        // Hide tooltip
        TooltipManager._instance.HideTooltip();

        // Hide subtitle
        barChart.HideSubtitle();

        // Update chart data for the top 5 delays
        UpdateTop5Delays(barChart);

        // Update chart title to "Top-5 Delay-Prone Airports"
        UpdateChartTitle("Top-5 Delay-Prone Airports", barChart);

        // Add labels under bars
        barChart.AddLabelsUnderBars(new string[]{"EBCV", "KBPT", "KLSF", "KTIK", "LFDM"});
    }
}

    void UpdateChart(PointScript nearestObj, BarChartScript barChart)
    {
        // Access the BarChartScript and update its data
        if (barChart != null)
        {
            // Query avg_delay for each year from airport_year_aggregated_delays using airportCode
            float[] newData = new float[5];

            for (int year = 2017; year <= 2021; year++)
            {
                double avgDelay = FetchAvgDelayFromYearAggregatedDelays(nearestObj.airportCode, year);
                newData[year - 2017] = (float)avgDelay;
            }

            // Update chart with the selected airport's average delays for each year
            barChart.UpdateData(newData);

            // Update chart title
            string chartTitle =  "Average Delay Per Year";
            barChart.UpdateChartTitle(chartTitle);

            // Update subtitle with airport name
            string subtitle = nearestObj.airportName;
            barChart.UpdateSubtitle(subtitle);
        }
    }

    void UpdateTop5Delays(BarChartScript barChart)
    {
        // Access the BarChartScript and update its data with the top 5 delays
        if (barChart != null)
        {
            // Query top 5 delays from aggregated_delays table
            float[] newData = FetchTop5Delays();

            // Update chart with the top 5 delays
            barChart.UpdateData(newData);
        }
    }

    void UpdateChartTitle(string title, BarChartScript barChart)
    {
        if (barChart != null)
        {
            barChart.UpdateChartTitle(title);
        }
    }

    void HideSubtitle(BarChartScript barChart)
    {
        if (barChart != null)
        {
            barChart.HideSubtitle();
        }
    }

    double FetchAvgDelayFromYearAggregatedDelays(string airportCode, int year)
    {
        double avgDelay = 0.0;

        // Use the same database connection code as in your MarkSpawner script
        string dbPath = "URI=file:" + Application.dataPath + "/../../aviation.db";
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        using (IDbConnection dbConnection = new System.Data.SQLite.SQLiteConnection(dbPath))
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        using (IDbConnection dbConnection = new Mono.Data.Sqlite.SqliteConnection(dbPath))
#endif
        {
            try
            {
                dbConnection.Open();
                string query = $"SELECT avg_delay FROM airport_year_aggregated_delays WHERE airport_code = '{airportCode}' AND year = {year}";
                IDbCommand dbCommand = dbConnection.CreateCommand();
                dbCommand.CommandText = query;

                object result = dbCommand.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                {
                    avgDelay = Convert.ToDouble(result);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error executing query: {e.Message}");
            }
            finally
            {
                if (dbConnection.State == ConnectionState.Open)
                    dbConnection.Close();
            }
        }

        return avgDelay;
    }

    float[] FetchTop5Delays()
    {
        float[] top5Delays = new float[5];

        // Use the same database connection code as in your MarkSpawner script
        string dbPath = "URI=file:" + Application.dataPath + "/../../aviation.db";
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        using (IDbConnection dbConnection = new System.Data.SQLite.SQLiteConnection(dbPath))
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        using (IDbConnection dbConnection = new Mono.Data.Sqlite.SqliteConnection(dbPath))
#endif
        {
            try
            {
                dbConnection.Open();
                string query = "SELECT avg_delay FROM aggregated_delays ORDER BY avg_delay DESC LIMIT 5";
                IDbCommand dbCommand = dbConnection.CreateCommand();
                dbCommand.CommandText = query;

                IDataReader reader = dbCommand.ExecuteReader();

                int index = 0;
                while (reader.Read() && index < 5)
                {
                    object avgDelayObject = reader["avg_delay"];
                    top5Delays[index++] = (avgDelayObject != DBNull.Value) ? Convert.ToSingle(avgDelayObject) : 0f;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error executing query: {e.Message}");
            }
            finally
            {
                if (dbConnection.State == ConnectionState.Open)
                    dbConnection.Close();
            }
        }

        return top5Delays;
    }
}
