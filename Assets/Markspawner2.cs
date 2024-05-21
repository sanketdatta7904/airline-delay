using UnityEngine;
using Microsoft.Maps.Unity;
using System;
using System.Data;
using System.Collections.Generic;
using TMPro;
using System.Linq;

// using System.Data.SQLite;
using UnityEngine.UI;
using System.Diagnostics.Tracing;

// import SQLite on windows platform
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using System.Data.SQLite;
#endif
// import SQLite on macOS platform
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
    using Mono.Data.Sqlite;
#endif

public class MarkSpawner2 : MonoBehaviour
{

    public Material Greencolor;
    public Material Redcolor;

    public Material Yellowcolor;

    public GameObject lineRendererPrefab;
    public GameObject mark;
    public GameObject markSmall;
    public GameObject markMedium;
    public GameObject markLarge;
    public GameObject parent;
    public GameObject[] allPoints;
    public static float mapZoom = 1.0f;
    public GameObject map;


    private double counter = 0;
    private double longitudeCenter;
    private double latitudeCenter;
    private double xOffSet = 0.0f;
    private double yOffSet = 0.0f;
    private static float selectedAvgDelay = 1;

    public InputField sourceAirportInputField;
    public InputField destinationAirportInputField;

    // Add a button to trigger the route search
    public Button searchRoutesButton;

    private float lineRendererWidth = 0.001f;
    private float benzierCurve = 0.2f;
    public TMP_Dropdown sourceAirportList; // Assign in Inspector

    public TMP_Dropdown destinationAirportList;

    public Button SearchButton;

    public string selectedSourceAirport;

    public string selectedDestinationAirport;

    // kd tree
    private static KdTree<PointScript> allPointsKd = new KdTree<PointScript>();
    // private string dbPath = "URI=file:/c/Users/Sanket PC/Desktop/group-2/aviation_new.db"; // Powerwall
    string dbPath = "URI=file:" + "D:/sqlite/aviation_new.db"; // Sanket


    // private List<LineRenderer> lineRenderers = new List<LineRenderer>();
    public class LineRendererEndpoints
    {
        public LineRenderer LineRenderer;
        public GameObject StartMarker;
        public GameObject EndMarker;
    }

    public class RouteData
    {
        public double SourceLatitude;
        public double SourceLongitude;
        public double DestLatitude;
        public double DestLongitude;
        public double AverageDelay;
        public double TrafficCount;

        public string SourceAirportName;
        public string DestAirportName;
        public string SourceCountry;
        public string DestCountry;
    }

    private List<LineRendererEndpoints> lineRendererEndpoints = new List<LineRendererEndpoints>();
    private Dictionary<string, RouteData> routeDataMap = new Dictionary<string, RouteData>();
    Dictionary<string, List<string>> airportReachabilityMap = new Dictionary<string, List<string>>();
    Dictionary<string, (string name, double Latitude, double Longitude, double AverageDelay, string country)> airportLocationMap = new Dictionary<string, (string, double, double, double, string)>();

    Dictionary<string, string> airportNameToCodeMap = new Dictionary<string, string>(); // Name to code mapping

    void Start()
    {
        getReachabilityMap();
        getAirportDetails();
        PopulateAirportsDropdown();
        // sourceAirportList.onValueChanged.AddListener();
        // YearDropdown.onValueChanged.AddListener(delegate { UpdateSelectedYear(); });
        GameObject SearchButtonObj = GameObject.Find("SearchButton");
        if (SearchButtonObj != null)
        {
            SearchButton = SearchButtonObj.GetComponent<Button>();
        }
        else
        {
            Debug.LogError("Dropdown GameObject not found.");
        }
        SearchButton.onClick.AddListener(OnSearchButtonClicked);
    }

private void getReachabilityMap()
{
    //string dbPath = "URI=file:" + "D:/sqlite/aviation_new.db";
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        IDbConnection dbConnection = new SQLiteConnection(dbPath);
#endif
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
    IDbConnection dbConnection = new SqliteConnection(dbPath);
#endif

        dbConnection.Open();
        string query = "SELECT * FROM airport_reachability;";
        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = query;
        IDataReader reader = dbCommand.ExecuteReader();
        while (reader.Read())
        {
            string sourceAirport = reader["source_airport_id"].ToString();
            string reachableAirportsString = reader["destination_airport_ids"].ToString();

            // Split the string of reachable airports into an array, trimming each entry
            var reachableAirports = reachableAirportsString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(airportCode => airportCode.Trim())
                                        .ToList();

            // Directly assign the list to the source airport key
            airportReachabilityMap[sourceAirport] = reachableAirports;
        }
        dbConnection.Close();
    }

    public void getAirportDetails()
    {
        //string dbPath = "URI=file:" + "D:/sqlite/aviation_new.db";
        // string dbPath = "D:/APVE23-24/Group%2/aviation.db";
        // either use SQLite on windows platform or Sqlite on macOS platform
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        IDbConnection dbConnection = new SQLiteConnection(dbPath);
#endif
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            IDbConnection dbConnection = new SqliteConnection(dbPath);
#endif
        dbConnection.Open();
        string query = "SELECT airport_code, airport_name, latitude, longitude, avg_delay, country FROM aggregated_delays;";
        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = query;
        IDataReader reader = dbCommand.ExecuteReader();
        while (reader.Read())
        {
            string code = reader["airport_code"].ToString();
            string name = reader["airport_name"].ToString();
            double latitude = double.Parse(reader["latitude"].ToString());
            double longitude = double.Parse(reader["longitude"].ToString());
            double averageDelay = reader.IsDBNull(reader.GetOrdinal("avg_delay")) ? 0.0 : double.Parse(reader["avg_delay"].ToString());
            string country = reader["country"].ToString();

            airportLocationMap[code] = (name, latitude, longitude, averageDelay, country);
            airportNameToCodeMap[name] = code;

        }
        dbConnection.Close();
    }
    private string GetAirportCodeFromName(string airportName)
    {
        if (airportNameToCodeMap.TryGetValue(airportName, out string code))
        {
            return code;
        }
        else
        {
            Debug.LogError($"Airport code not found for {airportName}");
            return null; // Or handle this case as appropriate
        }
    }

    void FetchAndDrawRoutesForSourceDestAirport(string sourceAirport, string targetAirport)
    {
        updateLocation();

        // First, get the airport codes from the names
        string sourceCode = GetAirportCodeFromName(sourceAirport);
        string destinationCode = GetAirportCodeFromName(targetAirport);



        // Check direct reachability
        if (airportReachabilityMap.ContainsKey(sourceCode) && airportReachabilityMap[sourceCode].Contains(destinationCode))
        {
            var source = airportLocationMap[sourceCode];
            var destination = airportLocationMap[destinationCode];
            string sourceAirportName = airportLocationMap.ContainsKey(sourceCode) ? airportLocationMap[sourceCode].name : "Unknown";
            string destAirportName = airportLocationMap.ContainsKey(destinationCode) ? airportLocationMap[destinationCode].name : "Unknown";

            string sourceCountry = airportLocationMap.ContainsKey(sourceCode) ? airportLocationMap[sourceCode].country : "Unknown";
            string destCountry = airportLocationMap.ContainsKey(destinationCode) ? airportLocationMap[destinationCode].country : "Unknown";
            double directRouteAvgDelay = (source.AverageDelay + destination.AverageDelay) / 2;
            SpawnMarkAtLatLong($"{sourceCode}-{destinationCode}", source.Latitude, source.Longitude, destination.Latitude, destination.Longitude, directRouteAvgDelay, 0, sourceAirportName, destAirportName, sourceCountry, destCountry);
        }

        // Check one-hop reachability
        foreach (var intermediateCode in airportReachabilityMap[sourceCode])
        {
            if (airportReachabilityMap.ContainsKey(intermediateCode) && airportReachabilityMap[intermediateCode].Contains(destinationCode))
            {
                var source = airportLocationMap[sourceCode];
                var intermediate = airportLocationMap[intermediateCode];
                var destination = airportLocationMap[destinationCode];
                double intermediateRouteAvgDelay = (source.AverageDelay + intermediate.AverageDelay) / 2;
                double destRouteAvgDelay = (intermediate.AverageDelay + destination.AverageDelay) / 2;

                string sourceAirportName = airportLocationMap.ContainsKey(sourceCode) ? airportLocationMap[sourceCode].name : "Unknown";

                string intermediateAirportName = airportLocationMap.ContainsKey(intermediateCode) ? airportLocationMap[intermediateCode].name : "Unknown";

                string destAirportName = airportLocationMap.ContainsKey(destinationCode) ? airportLocationMap[destinationCode].name : "Unknown";

                string sourceCountry = airportLocationMap.ContainsKey(sourceCode) ? airportLocationMap[sourceCode].country : "Unknown";
                string intermediateCountry = airportLocationMap.ContainsKey(intermediateCode) ? airportLocationMap[intermediateCode].country : "Unknown";
                string destCountry = airportLocationMap.ContainsKey(destinationCode) ? airportLocationMap[destinationCode].country : "Unknown";

                // Render source to intermediate
                SpawnMarkAtLatLong($"{sourceCode}-{intermediateCode}", source.Latitude, source.Longitude, intermediate.Latitude, intermediate.Longitude, intermediateRouteAvgDelay, 0, sourceAirportName, intermediateAirportName, sourceCountry, intermediateCountry);

                // Render intermediate to destination
                SpawnMarkAtLatLong($"{intermediateCode}-{destinationCode}", intermediate.Latitude, intermediate.Longitude, destination.Latitude, destination.Longitude, destRouteAvgDelay, 0, intermediateAirportName, destAirportName, intermediateCountry, destCountry);
            }
        }
    }


    void OnSearchButtonClicked()
    {
        ClearExistingRoutes();
        string selectedSourceAirport = sourceAirportList.options[sourceAirportList.value].text;
        string selectedDestinationAirport = destinationAirportList.options[destinationAirportList.value].text;

        if (selectedSourceAirport == "Source Airport" || selectedDestinationAirport == "Destination Airport")
        {
            Debug.Log("Please select both source and destination airport before searching.");
            return;
        }

        // Perform your search logic here using selectedCountry and selectedYear
        FetchAndDrawRoutesForSourceDestAirport(selectedSourceAirport, selectedDestinationAirport);
    }

    void ClearExistingRoutes()
    {
        // Example: Destroy all children of the parent GameObject that holds the markers and lines
        foreach (var endpoint in lineRendererEndpoints)
        {
            if (endpoint.StartMarker != null) Destroy(endpoint.StartMarker);
            if (endpoint.EndMarker != null) Destroy(endpoint.EndMarker);
            if (endpoint.LineRenderer != null) Destroy(endpoint.LineRenderer.gameObject); // Assuming LineRenderer is attached to a GameObject
        }

        // Clear any stored references if necessary
        lineRendererEndpoints.Clear();
        allPointsKd.Clear(); // Clear the KD-tree or reinitialize it
        routeDataMap.Clear();

        foreach (Transform child in parent.transform)
        {
            Destroy(child.gameObject);
        }
    }
    void PopulateAirportsDropdown()
    {
        // Assuming you have a method to get country names from your database or a predefined list
        //string dbPath = "URI=file:" + "D:/sqlite/aviation_new.db";
        // string dbPath = "D:/APVE23-24/Group%2/aviation.db";
        // either use SQLite on windows platform or Sqlite on macOS platform
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        IDbConnection dbConnection = new SQLiteConnection(dbPath);
#endif
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            IDbConnection dbConnection = new SqliteConnection(dbPath);
#endif
        List<string> sourceAirportNames = new List<string>();
        dbConnection.Open();
        string query = "SELECT DISTINCT airport_name from aggregated_delays WHERE length(airport_name)>0 ORDER BY airport_name ASC;";
        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = query;

        IDataReader reader = dbCommand.ExecuteReader();
        sourceAirportNames.Add("Source Airport");
        while (reader.Read())
        {
            sourceAirportNames.Add(DBNull.Value.Equals(reader["airport_name"]) ? string.Empty : reader["airport_name"].ToString());
        }
        GameObject sourceAirportDropdownObject = GameObject.Find("Source airport");
        if (sourceAirportDropdownObject != null)
        {
            sourceAirportList = sourceAirportDropdownObject.GetComponent<TMP_Dropdown>();
            if (sourceAirportList != null)
            {
                sourceAirportList.AddOptions(sourceAirportNames);
                sourceAirportList.value = 0; // Set to default option
                sourceAirportList.RefreshShownValue();
                // foreach (var item in countryNames)
                // {
                //     countryDropdownList.options.Add(new TMP_Dropdown.OptionData() { text = item });
                // }
                // Populate dropdown
            }
            else
            {
                Debug.LogError("Dropdown component not found on the object.");
            }
        }
        else
        {
            Debug.LogError("Dropdown GameObject not found.");
        }
        sourceAirportNames[0] = "Destination Airport";
        GameObject destinationAirportDropdownObject = GameObject.Find("Destination airport");
        if (destinationAirportDropdownObject != null)
        {
            destinationAirportList = destinationAirportDropdownObject.GetComponent<TMP_Dropdown>();
            if (destinationAirportList != null)
            {
                destinationAirportList.AddOptions(sourceAirportNames);
                destinationAirportList.value = 0; // Set to default option
                destinationAirportList.RefreshShownValue();
                // foreach (var item in countryNames)
                // {
                //     countryDropdownList.options.Add(new TMP_Dropdown.OptionData() { text = item });
                // }
                // Populate dropdown
            }
            else
            {
                Debug.LogError("Dropdown component not found on the object.");
            }
        }
        else
        {
            Debug.LogError("Dropdown GameObject not found.");
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

    public void SpawnMarkAtLatLong(string RouteID, double Source_latitude, double Source_longitude, double Dest_latitude, double Dest_longitude, double avgDelay, double trafficCount, string SourceAirport, string DestAirport, string SourceCountry, string DestCountry)
    {

        // GameObject mark = size == "small" ? markSmall : size == "medium" ? markMedium : markLarge;
        // GameObject mark = markSmall;
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

        LineScript lineScript = lineRendererObj.AddComponent<LineScript>();
        lineScript.routeID = RouteID;
        lineScript.averageDelay = avgDelay;
        lineScript.trafficCount = trafficCount; // Ensure trafficCount is cast to int if necessary
        lineScript.sourceAirport = SourceAirport;
        lineScript.destAirport = DestAirport;

        lineScript.sourceCountry = SourceCountry;

        lineScript.destCountry = DestCountry;

        Debug.Log($"Line created with RouteID: {lineScript.routeID}, AvgDelay: {lineScript.averageDelay}, TrafficCount: {lineScript.trafficCount}");

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
            if (avgDelay < 5)
            {
                lineRenderer.material = Greencolor;
            }
            else if (avgDelay >= 5 && avgDelay <= 25)
            {
                lineRenderer.material = Yellowcolor;
            }
            else if (avgDelay > 25)
            {
                lineRenderer.material = Redcolor;
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

