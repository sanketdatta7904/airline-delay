using UnityEngine;
using Microsoft.Maps.Unity;
using System;
using System.Data;
using System.Collections.Generic;

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

    public Material greenMaterial;
    public Material redMaterial;

    public Material yellowcolor;

    public GameObject lineRendererPrefab;
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

    private float lineRendererWidth = 0.001f;
    private float benzierCurve = 0.05f;

    // kd tree
    private static KdTree<PointScript> allPointsKd = new KdTree<PointScript>();

    // private List<LineRenderer> lineRenderers = new List<LineRenderer>();
    public class LineRendererEndpoints
    {
        public LineRenderer LineRenderer;
        public GameObject StartMarker;
        public GameObject EndMarker;
    }
    private List<LineRendererEndpoints> lineRendererEndpoints = new List<LineRendererEndpoints>();

    void Start()
    {
        updateLocation();

        string dbPath = "URI=file:" + "D:/sqlite/aviation.db";
        // string dbPath = "D:/APVE23-24/Group%2/aviation.db";
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
            //string query = "SELECT * FROM aggregated_delays";
            string query = "SELECT * FROM route_delay_quarterly LIMIT 200";

            IDbCommand dbCommand = dbConnection.CreateCommand();
            dbCommand.CommandText = query;

            IDataReader reader = dbCommand.ExecuteReader();
            while (reader.Read())
            {
                string RouteID = DBNull.Value.Equals(reader["RouteID"]) ? string.Empty : reader["RouteID"].ToString();

                double Source_latitude = DBNull.Value.Equals(reader["Source_latitude"]) ? 0.0 : Convert.ToDouble(reader["Source_latitude"]);
                double Source_longitude = DBNull.Value.Equals(reader["Source_longitude"]) ? 0.0 : Convert.ToDouble(reader["Source_longitude"]);
                double Dest_latitude = DBNull.Value.Equals(reader["Dest_latitude"]) ? 0.0 : Convert.ToDouble(reader["Dest_latitude"]);
                double Dest_longitude = DBNull.Value.Equals(reader["Dest_longitude"]) ? 0.0 : Convert.ToDouble(reader["Dest_longitude"]);
                double avgDelay = DBNull.Value.Equals(reader["Average_Delay"]) ? 0.0 : Convert.ToDouble(reader["Average_Delay"]);



                string sizeString = avgDelay > 10 ? "large" : avgDelay > 0 ? "medium" : "small";


                SpawnMarkAtLatLong(RouteID, Source_latitude, Source_longitude, Dest_latitude, Dest_longitude, avgDelay, sizeString);
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
     * @param airportType - 
     */
    public void SpawnMarkAtLatLong(string RouteID, double Source_latitude, double Source_longitude, double Dest_latitude, double Dest_longitude, double avgDelay, string sizeString)
    {

        // GameObject mark = size == "small" ? markSmall : size == "medium" ? markMedium : markLarge;
        GameObject mark = markMedium;
        Vector3 sourcePos = GetPositionFromLatLon(Source_latitude, Source_longitude);
        Vector3 destPos = GetPositionFromLatLon(Dest_latitude, Dest_longitude);


        GameObject markInstance1 = Instantiate(mark, Vector3.zero, Quaternion.identity);

        markInstance1.transform.parent = parent.transform;
        markInstance1.GetComponent<PointScript>().longitude = Source_longitude;
        markInstance1.GetComponent<PointScript>().latitude = Source_latitude;
        markInstance1.GetComponent<PointScript>().avgDelay = avgDelay;
        markInstance1.GetComponent<PointScript>().airportName = RouteID.Split('-')[0];
        markInstance1.GetComponent<PointScript>().Redraw(mapZoom);

        // add the gameObject to the kd tree
        allPointsKd.Add(markInstance1.GetComponent<PointScript>());



        GameObject markInstance2 = Instantiate(mark, Vector3.zero, Quaternion.identity);

        markInstance2.transform.parent = parent.transform;
        markInstance2.GetComponent<PointScript>().longitude = Dest_longitude;
        markInstance2.GetComponent<PointScript>().latitude = Dest_latitude;
        markInstance2.GetComponent<PointScript>().avgDelay = avgDelay;
        markInstance2.GetComponent<PointScript>().airportName = RouteID.Split('-')[1];
        markInstance2.GetComponent<PointScript>().Redraw(mapZoom);

        // add the gameObject to the kd tree
        allPointsKd.Add(markInstance2.GetComponent<PointScript>());


        GameObject lineRendererObj = Instantiate(lineRendererPrefab, Vector3.zero, Quaternion.identity);


        LineRenderer lineRenderer = lineRendererObj.GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            int curveSegments = 20;
            float curveHeight = benzierCurve; // Adjust the height of the curve
            Vector3[] bezierPoints = CalculateBezierPoints(markInstance1, markInstance2, curveHeight, curveSegments);

            lineRenderer.positionCount = bezierPoints.Length;
            lineRenderer.SetPositions(bezierPoints);

            lineRenderer.startWidth = lineRendererWidth;
            lineRenderer.endWidth = lineRendererWidth;
            if (sizeString == "large")
            {
                lineRenderer.material = redMaterial;
            }
            else if (sizeString == "medium")
            {
                lineRenderer.material = yellowcolor;
            }
            else
            {
                lineRenderer.material = greenMaterial;
            }
            lineRendererEndpoints.Add(new LineRendererEndpoints
            {
                LineRenderer = lineRenderer,
                StartMarker = markInstance1,
                EndMarker = markInstance2
            });
        }
        else
        {
            Debug.LogError("LineRenderer component not found on the instantiated object.");
        }
    }

    void Update()
    {
        bool zoomChanged = mapZoom != map.GetComponent<MapRenderer>().ZoomLevel;
        if (zoomChanged ||
            longitudeCenter != map.GetComponent<MapRenderer>().Center.LongitudeInDegrees ||
            latitudeCenter != map.GetComponent<MapRenderer>().Center.LatitudeInDegrees)
        {
            updateLocation();
            foreach (var endpoint in lineRendererEndpoints)
            {
                if (endpoint.LineRenderer != null)
                {
                    int curveSegments = 20;
                    float curveHeight = benzierCurve;
                    Vector3[] bezierPoints = CalculateBezierPoints(endpoint.StartMarker, endpoint.EndMarker, curveHeight, curveSegments);
                    endpoint.LineRenderer.SetPositions(bezierPoints);
                }
            }
        }

        if (zoomChanged)
        {
            // iterate over the points kd tree and redraw the points with the new zoom
            foreach (PointScript point in allPointsKd)
            {
                point.Redraw(mapZoom);
            }
            foreach (var endpoint in lineRendererEndpoints)
            {
                if (endpoint.LineRenderer != null)
                {
                    int curveSegments = 20;
                    float curveHeight = benzierCurve;
                    Vector3[] bezierPoints = CalculateBezierPoints(endpoint.StartMarker, endpoint.EndMarker, curveHeight, curveSegments);
                    endpoint.LineRenderer.SetPositions(bezierPoints);
                }
            }
        }

    }

    Vector3[] CalculateBezierPoints(GameObject start, GameObject end, float height, int segments)
    {
        Vector3[] points = new Vector3[segments];
        Vector3 p0 = start.transform.position;
        Vector3 p2 = end.transform.position;
        Vector3 p1 = (p0 + p2) / 2 + Vector3.up * height;

        for (int i = 0; i < segments; i++)
        {
            float t = i / (segments - 1f);
            points[i] = CalculateBezierPoint(t, p0, p1, p2);
        }

        return points;
    }

    Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 p = uu * p0;
        p += 2 * u * t * p1;
        p += tt * p2;

        return p;
    }

    private Vector3 GetPositionFromLatLon(double latitude, double longitude)
    {
        float x = (float)CoordinatConverter.NormalizeLongitudeWebMercator(longitude, mapZoom);
        float y = (float)CoordinatConverter.NormalizeLatitudeWebMercator(latitude, mapZoom);
        return new Vector3(x, y, 0.0f);
    }

    public static PointScript getClosestPoint(Vector3 position)
    {
        PointScript nearestObj = allPointsKd.FindClosest(position);
        return nearestObj;
    }
}

