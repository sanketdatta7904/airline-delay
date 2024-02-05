using UnityEngine;
using Microsoft.Maps.Unity;
using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine.UI;

public class MarkSpawner : MonoBehaviour
{
    public GameObject mark;
    public GameObject markSmall;
    public GameObject markMedium;
    public GameObject markLarge;
    public GameObject parent;
    public GameObject[] allPoints;
    public static float mapZoom = 1.0f;
    public GameObject map;

    private double longitudeCenter;
    private double latitudeCenter;
    private double xOffSet = 0.0f;
    private double yOffSet = 0.0f;

    private static KdTree<PointScript> allPointsKd = new KdTree<PointScript>();
    private static string selectedAirportType = "All";
    private static float selectedAvgDelay = 1;
    private static string selectedAirportName = "";

    public InputField airportNameInput;

    void Start()
    {
        updateLocation();

        string dbPath = "URI=file:" + Application.dataPath + "/../../aviation.db";
        Debug.Log(dbPath);

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
            string query = "SELECT * FROM aggregated_delays";
            IDbCommand dbCommand = dbConnection.CreateCommand();
            dbCommand.CommandText = query;

            IDataReader reader = dbCommand.ExecuteReader();
            while (reader.Read())
            {
                string airportCode = DBNull.Value.Equals(reader["airport_code"]) ? string.Empty : reader["airport_code"].ToString();
                string airportName = DBNull.Value.Equals(reader["airport_name"]) ? string.Empty : reader["airport_name"].ToString();
                string airportType = DBNull.Value.Equals(reader["ades_type"]) ? string.Empty : reader["ades_type"].ToString();
                double latitude = DBNull.Value.Equals(reader["latitude"]) ? 0.0 : Convert.ToDouble(reader["latitude"]);
                double longitude = DBNull.Value.Equals(reader["longitude"]) ? 0.0 : Convert.ToDouble(reader["longitude"]);
                double avgDelay = DBNull.Value.Equals(reader["avg_delay"]) ? 0.0 : Convert.ToDouble(reader["avg_delay"]);

                string sizeString = airportType == "large_airport" ? "large" : airportType == "medium_airport" ? "medium" : airportType == "small_airport" ? "small" : "other";

                if ((selectedAirportType == "All" || airportType == selectedAirportType) &&
                    (selectedAvgDelay == 1 || (avgDelay >= selectedAvgDelay && avgDelay <= 10)))
                {
                    SpawnMarkAtLatLong(latitude, longitude, airportName, airportCode, sizeString, avgDelay, airportType);
                }
            }
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

        airportNameInput.onValueChanged.AddListener(OnAirportNameInputChange);
    }

    void Update()
    {
        bool zoomChanged = mapZoom != map.GetComponent<MapRenderer>().ZoomLevel;
        if (zoomChanged ||
            longitudeCenter != map.GetComponent<MapRenderer>().Center.LongitudeInDegrees ||
            latitudeCenter != map.GetComponent<MapRenderer>().Center.LatitudeInDegrees)
        {
            updateLocation();
        }

        if (zoomChanged)
        {
            foreach (PointScript point in allPointsKd)
            {
                point.Redraw(mapZoom);
            }
        }
    }

    private void updateLocation()
    {
        mapZoom = map.GetComponent<MapRenderer>().ZoomLevel;
        longitudeCenter = map.GetComponent<MapRenderer>().Center.LongitudeInDegrees;
        latitudeCenter = map.GetComponent<MapRenderer>().Center.LatitudeInDegrees;
        xOffSet = CoordinatConverter.NormalizeLongitudeWebMercator(longitudeCenter, mapZoom);
        yOffSet = CoordinatConverter.NormalizeLatitudeWebMercator(latitudeCenter, mapZoom);

        parent.transform.position = new Vector3((float)-xOffSet, (float)-yOffSet, 0.0f);
    }

    public void SpawnMarkAtLatLong(double latitude, double longitude, string airportName, string airportCode, string size, double avgDelay, string airportType)
    {
        float x = (float)CoordinatConverter.NormalizeLongitudeWebMercator(longitude, mapZoom);
        float y = (float)CoordinatConverter.NormalizeLatitudeWebMercator(latitude, mapZoom);
        float sizeScale = size == "large" ? 0.01f : size == "medium" ? 0.008f : size == "small" ? 0.006f : 0.004f;
        UnityEngine.Color color = avgDelay > 50 ? Color.red : avgDelay > 10 ? Color.yellow : Color.green;

        GameObject markInstance = Instantiate(markLarge, Vector3.zero, Quaternion.identity);
        markInstance.transform.parent = parent.transform;
        markInstance.GetComponent<PointScript>().latitude = latitude;
        markInstance.GetComponent<PointScript>().longitude = longitude;
        markInstance.GetComponent<PointScript>().airportName = airportName;
        markInstance.GetComponent<PointScript>().airportCode = airportCode;
        markInstance.GetComponent<PointScript>().size = size;
        markInstance.GetComponent<PointScript>().airportType = airportType;
        markInstance.GetComponent<PointScript>().avgDelay = avgDelay;
        markInstance.GetComponent<PointScript>().Redraw(mapZoom);

        if (!string.IsNullOrEmpty(selectedAirportName) && airportName.Equals(selectedAirportName, StringComparison.OrdinalIgnoreCase))
        {
            sizeScale *= 75f;
            markInstance.GetComponent<SpriteRenderer>().color = Color.black;
        }
        else
        {
            markInstance.GetComponent<SpriteRenderer>().color = color;
        }

        markInstance.transform.localScale = new Vector3(sizeScale, sizeScale, sizeScale);

        allPointsKd.Add(markInstance.GetComponent<PointScript>());

        markInstance.SetActive(selectedAirportType == "All" || airportType == selectedAirportType);
        markInstance.SetActive(selectedAvgDelay == 1f || avgDelay >= selectedAvgDelay && avgDelay <= 10f);
    }

    public static void SetSelectedAirportType(string airportType)
    {
        selectedAirportType = airportType;
    }

    public static void FilterAirportsByAvgDelay(float avgDelay)
    {
        selectedAvgDelay = avgDelay;

        foreach (PointScript point in allPointsKd)
        {
            float pointAvgDelay = (float)point.avgDelay;

            point.gameObject.SetActive(
                (selectedAirportType == "All" || point.airportType == selectedAirportType) &&
                (selectedAvgDelay == -1f || (pointAvgDelay >= selectedAvgDelay && pointAvgDelay <= 100f))
            );
        }
    }

    private void OnAirportNameInputChange(string value)
    {
        selectedAirportName = value;

        foreach (PointScript point in allPointsKd)
        {
            double pointAvgDelay = point.avgDelay;
            float sizeScale = GetSizeScale(point.airportType);

            if (!string.IsNullOrEmpty(selectedAirportName) && point.airportName.Equals(selectedAirportName, StringComparison.OrdinalIgnoreCase))
            {
                sizeScale *= 2f;
                point.GetComponent<SpriteRenderer>().color = Color.black;
            }
            else
            {
                point.GetComponent<SpriteRenderer>().color = GetColorForAvgDelay(point.avgDelay);
            }

            point.gameObject.SetActive(
                (selectedAirportType == "All" || point.airportType == selectedAirportType) &&
                (selectedAvgDelay == -1 || (pointAvgDelay >= selectedAvgDelay && pointAvgDelay <= 100))
            );

            point.transform.localScale = new Vector3(sizeScale, sizeScale, sizeScale);
        }
    }

    private float GetSizeScale(string airportType)
    {
        return airportType == "large_airport" ? 0.01f : airportType == "medium_airport" ? 0.008f : airportType == "small_airport" ? 0.006f : 0.004f;
    }

    private UnityEngine.Color GetColorForAvgDelay(double avgDelay)
    {
        return avgDelay > 50 ? Color.red : avgDelay > 10 ? Color.yellow : Color.green;
    }
    public static PointScript getClosestPoint(Vector3 position)
{
    PointScript nearestObj = allPointsKd.FindClosest(position);
    return nearestObj;
}

public static void FilterAirportsByType(string airportType)
{
    selectedAirportType = airportType;

    foreach (PointScript point in allPointsKd)
    {
        double pointAvgDelay = point.avgDelay;

        // Check if the point should be visible based on both airport type and average delay
        bool shouldShow = (selectedAirportType == "All" || point.airportType == selectedAirportType) &&
                          (selectedAvgDelay == -1 || (pointAvgDelay >= selectedAvgDelay && pointAvgDelay <= 100));

        point.gameObject.SetActive(shouldShow);
    }
}

    public static Vector3 getRelativePosition(double latitude, double longitude)
    {
        var x = CoordinatConverter.NormalizeLatitudeWebMercator(latitude, mapZoom);
        var y = CoordinatConverter.NormalizeLongitudeWebMercator(longitude, mapZoom);
        return new Vector3((float)x, (float)y, 0.0f);
    }



}