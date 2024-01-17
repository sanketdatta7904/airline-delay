using UnityEngine;
using Microsoft.Maps.Unity;
using System;
using System.Data;
// using System.Data.SQLite;
using UnityEngine.UI;
// import SQLite on windows platform
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    using System.Data.SQLite;
#endif
// import SQLite on macOS platform
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
    using Mono.Data.Sqlite;
#endif

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


    void Start()
    {
        updateLocation();

        string dbPath = "URI=file:" + Application.dataPath + "/Aviation111.db";
        // either use SQLite on windows platform or Sqlite on macOS platform
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            IDbConnection dbConnection = new SQLiteConnection(dbPath);
#endif
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            IDbConnection dbConnection = new SqliteConnection(dbPath);
#endif
        //IDbConnection dbConnection = new SQLiteConnection(dbPath);

        try
        {
            dbConnection.Open();

            // Query to select latitude and longitude from the aggregated_delays table
            string query = "SELECT * FROM aggregated_delays";
            IDbCommand dbCommand = dbConnection.CreateCommand();
            dbCommand.CommandText = query;

            IDataReader reader = dbCommand.ExecuteReader();
            while (reader.Read())
            {
                // iterate over all the rows in the table
                //for (int i = 0; i < reader.FieldCount; i++)
                //{
                //    Debug.Log(reader.GetName(i) + ": " + reader[i]);
                //}

                string airportCode = DBNull.Value.Equals(reader["airport_code"]) ? string.Empty : reader["airport_code"].ToString();
                string airportName = DBNull.Value.Equals(reader["airport_name"]) ? string.Empty : reader["airport_name"].ToString();
                double latitude = DBNull.Value.Equals(reader["latitude"]) ? 0.0 : Convert.ToDouble(reader["latitude"]);
                double longitude = DBNull.Value.Equals(reader["longitude"]) ? 0.0 : Convert.ToDouble(reader["longitude"]);
                double avgDelay = DBNull.Value.Equals(reader["avg_delay"]) ? 0.0 : Convert.ToDouble(reader["avg_delay"]);



                string sizeString = avgDelay>10? "large": avgDelay>0? "medium": "small";


                // Spawn a mark for each latitude and longitude
                SpawnMarkAtLatLong(latitude, longitude, airportName, airportCode, sizeString, avgDelay);
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

    /**
     * Spawn a mark at the given latitude and longitude
     * Each mark represents an airport and has a name, code and size
     * @param latitude - the latitude of the airport
     * @param longitude - the longitude of the airport
     * @param airportName - the name of the airport
     * @param airportCode - the code of the airport
     * @param size - the size of the airport (small, medium, large)
     */
    public void SpawnMarkAtLatLong(double latitude, double longitude, string airportName, string airportCode, string size, double avgDelay)
    {
        float x = (float)CoordinatConverter.NormalizeLongitudeWebMercator(longitude, mapZoom);
        float y = (float)CoordinatConverter.NormalizeLatitudeWebMercator(latitude, mapZoom);
        GameObject mark = size == "small" ? markSmall : size == "medium" ? markMedium : markLarge;

        GameObject markInstance = Instantiate(mark, Vector3.zero, Quaternion.identity);

        markInstance.transform.parent = parent.transform;
        markInstance.GetComponent<PointScript>().latitude = latitude;
        markInstance.GetComponent<PointScript>().longitude = longitude;
        markInstance.GetComponent<PointScript>().airportName = airportName;
        markInstance.GetComponent<PointScript>().airportCode = airportCode;
        markInstance.GetComponent<PointScript>().size = size;
        markInstance.GetComponent<PointScript>().avgDelay = avgDelay;
        markInstance.GetComponent<PointScript>().Redraw(mapZoom);
        
        Array.Resize(ref allPoints, allPoints.Length + 1);
        allPoints[allPoints.Length - 1] = markInstance;
    }
}
