using UnityEngine;
using Microsoft.Maps.Unity;
using System;
using System.Data;
using System.Data.SQLite;

public class MarkSpawner : MonoBehaviour
{
    public GameObject mark;
    public GameObject markSmall;
    public GameObject markMedium;
    public GameObject markLarge;
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
                /*object averageDelayObject = reader["avg_delay"];
                object airportNameObject = reader["airport_name"];
                object airportCodeObject = reader["airport_code"];*/

                // Check for DBNull before conversion
                double latitude = DBNull.Value.Equals(latitudeObject) ? 0.0 : Convert.ToDouble(latitudeObject);
                double longitude = DBNull.Value.Equals(longitudeObject) ? 0.0 : Convert.ToDouble(longitudeObject);
                //double delay = DBNull.Value.Equals(averageDelayObject) ? 0.0 : Convert.ToDouble(averageDelayObject);
                //string airportName = DBNull.Value.Equals(airportNameObject) ? "" : Convert.ToString(airportNameObject);
                //string airportCode = DBNull.Value.Equals(airportCodeObject) ? "" : Convert.ToString(airportCodeObject);
                //double size = Convert.ToDouble(airportCode) % 3;
                //string sizeString = size == 0 ? "small" : size == 1 ? "medium" : "large";
                int random = UnityEngine.Random.Range(0, 3);
                string sizeString = random == 0 ? "small" : random == 1 ? "medium" : "large";

                // Spawn a mark for each latitude and longitude
                SpawnMarkAtLatLong(latitude, longitude, "name", "code", sizeString);
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

    /**
     * Spawn a mark at the given latitude and longitude
     * Each mark represents an airport and has a name, code and size
     * @param latitude - the latitude of the airport
     * @param longitude - the longitude of the airport
     * @param airportName - the name of the airport
     * @param airportCode - the code of the airport
     * @param size - the size of the airport (small, medium, large)
     */
    public void SpawnMarkAtLatLong(double latitude, double longitude, string airportName, string airportCode, string size)
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
        markInstance.GetComponent<PointScript>().Redraw(mapZoom);

        Array.Resize(ref allPoints, allPoints.Length + 1);
        allPoints[allPoints.Length - 1] = markInstance;
    }
}
