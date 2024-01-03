using UnityEngine;
using Microsoft.Maps.Unity;
using System;
using System.Data;
using System.Data.SQLite;

public class MarkSpawner : MonoBehaviour
{
    public GameObject mark;
    public GameObject parent;
    public GameObject[] allPoints;
    private double longitudeCenter;
    private double latitudeCenter;
    public static float mapZoom = 1.0f;

    private double xOffSet = 0.0f;
    private double yOffSet = 0.0f;

    public GameObject map;

    void Start()
    {
        updateLocation();

        string dbPath = "URI=file:" + Application.dataPath + "/Aviation111.db";
        IDbConnection dbConnection = new SQLiteConnection(dbPath);

        try
        {
            dbConnection.Open();

            // Query to select latitude and longitude from the aggregated_delays table
            string query = "SELECT latitude, longitude FROM aggregated_delays";
            IDbCommand dbCommand = dbConnection.CreateCommand();
            dbCommand.CommandText = query;

            IDataReader reader = dbCommand.ExecuteReader();
            while (reader.Read())
            {
                object latitudeObject = reader["latitude"];
                object longitudeObject = reader["longitude"];

                // Check for DBNull before conversion
                double latitude = DBNull.Value.Equals(latitudeObject) ? 0.0 : Convert.ToDouble(latitudeObject);
                double longitude = DBNull.Value.Equals(longitudeObject) ? 0.0 : Convert.ToDouble(longitudeObject);

                // Spawn a mark for each latitude and longitude
                SpawnMarkAtLatLong(latitude, longitude, mapZoom);
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

    void Update()
    {
        if (mapZoom != map.GetComponent<MapRenderer>().ZoomLevel ||
            longitudeCenter != map.GetComponent<MapRenderer>().Center.LongitudeInDegrees ||
            latitudeCenter != map.GetComponent<MapRenderer>().Center.LatitudeInDegrees)
        {
            updateLocation();

            foreach (GameObject point in allPoints)
            {
                point.GetComponent<PointScript>().Redraw(mapZoom);
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

    public void SpawnMarkAtLatLong(double latitude, double longitude, double zoom)
    {
        float x = (float)CoordinatConverter.NormalizeLongitudeWebMercator(longitude, zoom);
        float y = (float)CoordinatConverter.NormalizeLatitudeWebMercator(latitude, zoom);

        GameObject markInstance = Instantiate(mark, Vector3.zero, Quaternion.identity);

        markInstance.transform.parent = parent.transform;
        markInstance.GetComponent<PointScript>().latitude = latitude;
        markInstance.GetComponent<PointScript>().longitude = longitude;
        markInstance.GetComponent<PointScript>().Redraw(mapZoom);

        Array.Resize(ref allPoints, allPoints.Length + 1);
        allPoints[allPoints.Length - 1] = markInstance;
    }
}
