using UnityEngine;
using Microsoft.Maps.Unity;
using System;
using System.Data;
using System.Collections.Generic;
using TMPro;

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

public class MarkSpawner1 : MonoBehaviour
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
    public TMP_Dropdown countryDropdownList; // Assign in Inspector

    public TMP_Dropdown destCountryDropdown;

    public TMP_Dropdown YearDropdown;

    public Button SearchButton;

    public string selectedCountry;

    public string selectedYear;

    // kd tree
    private static KdTree<PointScript> allPointsKd = new KdTree<PointScript>();

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

        public string SourceAirport;

        public string DestAirport;

        public string SourceCountry;

        public string DestCountry;
    }

    private List<LineRendererEndpoints> lineRendererEndpoints = new List<LineRendererEndpoints>();
    private Dictionary<string, RouteData> routeDataMap = new Dictionary<string, RouteData>();

    public static Dictionary<string, (string name, double Latitude, double Longitude)> airportDetailsMap = new Dictionary<string, (string, double, double)>();

    Dictionary<string, (string name, double Latitude, double Longitude)> airportLocationMap = new Dictionary<string, (string, double, double)>();

    Dictionary<string, string> airportNameToCodeMap = new Dictionary<string, string>();

    void Start()
    {
        getAirportDetails();
        PopulateCountryDropdown();
        countryDropdownList.onValueChanged.AddListener(UpdateSelectedCountry);
        YearDropdown.onValueChanged.AddListener(delegate { UpdateSelectedYear(); });
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
    // private void CountryDropdownValueChanged(int index)
    // {

    //     string selectedCountry = countryDropdownList.options[index].text;
    //     if (selectedCountry == "Source Country")
    //     {
    //         ClearExistingRoutes();
    //         return;
    //     }  // Skip if the default option is selected

    //     ClearExistingRoutes(); // Implement this method to clear existing lines and markers from the map
    //     FetchAndDrawRoutesForCountry(selectedCountry);
    // }
    void UpdateSelectedCountry(int index)
    {
        selectedCountry = countryDropdownList.options[index].text;
        Debug.Log("Selected country: " + selectedCountry);
    }
    public void getAirportDetails()
    {
        string dbPath = "URI=file:" + "D:/sqlite/aviation_new.db";
        // string dbPath = "D:/APVE23-24/Group%2/aviation.db";
        // either use SQLite on windows platform or Sqlite on macOS platform
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        IDbConnection dbConnection = new SQLiteConnection(dbPath);
#endif
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            IDbConnection dbConnection = new SqliteConnection(dbPath);
#endif
        dbConnection.Open();
        string query = "SELECT airport_code, airport_name, latitude, longitude FROM aggregated_delays;";
        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = query;
        IDataReader reader = dbCommand.ExecuteReader();
        while (reader.Read())
        {
            string code = reader["airport_code"].ToString();
            string name = reader["airport_name"].ToString();
            double latitude = double.Parse(reader["latitude"].ToString());
            double longitude = double.Parse(reader["longitude"].ToString());
            airportLocationMap[code] = (name, latitude, longitude);
            airportNameToCodeMap[name] = code;

        }
        dbConnection.Close();
    }
    void UpdateSelectedYear()
    {
        selectedYear = YearDropdown.options[YearDropdown.value].text;
        Debug.Log("Selected year: " + selectedYear);
    }
    void OnSearchButtonClicked()
    {
        ClearExistingRoutes();
        string selectedSourceCountry = countryDropdownList.options[countryDropdownList.value].text;
        string selectedDestCountry = destCountryDropdown.options[destCountryDropdown.value].text;
        string selectedYear = YearDropdown.options[YearDropdown.value].text;

        if (selectedYear == "Year" || (selectedSourceCountry == "Source Country" && selectedDestCountry == "Destination Country"))
        {
            Debug.Log("Please select both a country and a year before searching.");
            return;
        }
        string query = $"SELECT * FROM route_delay_quarterly WHERE Year = '{selectedYear}'";

        // Check if source country is provided and not the placeholder/default value.
        if (selectedSourceCountry != "Source Country")
        {
            query += $" AND Source_country = '{selectedSourceCountry}'";
        }

        // Check if destination country is provided and not the placeholder/default value.
        if (selectedDestCountry != "Destination Country")
        {
            query += $" AND Dest_country = '{selectedDestCountry}'";
        }

        // Perform your search logic here using selectedCountry and selectedYear
        FetchAndDrawRoutesForCountryAndYear(query);
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
    void PopulateCountryDropdown()
    {
        // Assuming you have a method to get country names from your database or a predefined list
        string dbPath = "URI=file:" + "D:/sqlite/aviation_new.db";
        // string dbPath = "D:/APVE23-24/Group%2/aviation.db";
        // either use SQLite on windows platform or Sqlite on macOS platform
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        IDbConnection dbConnection = new SQLiteConnection(dbPath);
#endif
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            IDbConnection dbConnection = new SqliteConnection(dbPath);
#endif
        List<string> countryNames = new List<string>();
        dbConnection.Open();
        string query = "SELECT DISTINCT country FROM aggregated_delays WHERE length(country) > 0 ORDER BY country ASC;";
        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = query;

        IDataReader reader = dbCommand.ExecuteReader();
        countryNames.Add("Source Country");
        while (reader.Read())
        {
            countryNames.Add(DBNull.Value.Equals(reader["country"]) ? string.Empty : reader["country"].ToString());
        }
        GameObject dropdownObject = GameObject.Find("DropdownCountry");
        if (dropdownObject != null)
        {
            countryDropdownList = dropdownObject.GetComponent<TMP_Dropdown>();
            if (countryDropdownList != null)
            {
                countryDropdownList.AddOptions(countryNames);
                countryDropdownList.value = 0; // Set to default option
                countryDropdownList.RefreshShownValue();

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
        countryNames[0] = "Destination Country";
        GameObject destdropdownObject = GameObject.Find("DestDropdownCountry");
        if (destdropdownObject != null)
        {
            destCountryDropdown = destdropdownObject.GetComponent<TMP_Dropdown>();
            if (destCountryDropdown != null)
            {
                destCountryDropdown.AddOptions(countryNames);
                destCountryDropdown.value = 0; // Set to default option
                destCountryDropdown.RefreshShownValue();
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

        GameObject yearDropdown = GameObject.Find("DropdownYear");
        if (yearDropdown != null)
        {
            YearDropdown = yearDropdown.GetComponent<TMP_Dropdown>();
        }
        else
        {
            Debug.LogError("Dropdown GameObject not found.");
        }

    }


    void FetchAndDrawRoutesForCountryAndYear(string query)
    {
        updateLocation();

        //string dbPath = "URI=file:" + "D:/sqlite/aviation_new.db";
        string dbPath = "URI=file:" + Application.dataPath + "/../../aviation.db";

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



            IDbCommand dbCommand = dbConnection.CreateCommand();
            dbCommand.CommandText = query;

            IDataReader reader = dbCommand.ExecuteReader();
            while (reader.Read())
            {
                counter += 1;
                string routeID = reader.IsDBNull(reader.GetOrdinal("RouteID")) ? string.Empty : reader.GetString(reader.GetOrdinal("RouteID"));
                string[] airports = routeID.Split('-');
                Array.Sort(airports);
                string standardizedRouteID = string.Join("-", airports);
                // Retrieve airport names using codes
                string[] airportCodes = routeID.Split('-');
                string sourceCode = airportCodes[0];
                string destCode = airportCodes[1];

                string sourceAirportName = airportLocationMap.ContainsKey(sourceCode) ? airportLocationMap[sourceCode].name : "Unknown";
                string destAirportName = airportLocationMap.ContainsKey(destCode) ? airportLocationMap[destCode].name : "Unknown";

                // Directly fetch country names from the database
                string sourceCountry = reader["source_country"].ToString();
                string destCountry = reader["dest_country"].ToString();

                RouteData data = new RouteData
                {
                    SourceLatitude = reader.IsDBNull(reader.GetOrdinal("Source_latitude")) ? 0.0 : reader.GetDouble(reader.GetOrdinal("Source_latitude")),
                    SourceLongitude = reader.IsDBNull(reader.GetOrdinal("Source_longitude")) ? 0.0 : reader.GetDouble(reader.GetOrdinal("Source_longitude")),
                    DestLatitude = reader.IsDBNull(reader.GetOrdinal("Dest_latitude")) ? 0.0 : reader.GetDouble(reader.GetOrdinal("Dest_latitude")),
                    DestLongitude = reader.IsDBNull(reader.GetOrdinal("Dest_longitude")) ? 0.0 : reader.GetDouble(reader.GetOrdinal("Dest_longitude")),
                    AverageDelay = reader.IsDBNull(reader.GetOrdinal("Average_Delay")) ? 0.0 : reader.GetDouble(reader.GetOrdinal("Average_Delay")),
                    TrafficCount = reader.IsDBNull(reader.GetOrdinal("Traffic_Count")) ? 0 : reader.GetDouble(reader.GetOrdinal("Traffic_Count")),
                    SourceAirport = sourceAirportName,
                    DestAirport = destAirportName,
                    SourceCountry = sourceCountry,
                    DestCountry = destCountry,
                };

                if (!routeDataMap.ContainsKey(standardizedRouteID))
                {
                    routeDataMap.Add(standardizedRouteID, data);
                }
                else
                {
                    var existingData = routeDataMap[standardizedRouteID];
                    double oldTrafficCount = existingData.TrafficCount;
                    double oldTotalDelay = existingData.AverageDelay * oldTrafficCount;

                    existingData.TrafficCount += data.TrafficCount;

                    double newTotalDelay = oldTotalDelay + data.AverageDelay * data.TrafficCount;
                    existingData.AverageDelay = newTotalDelay / existingData.TrafficCount;

                    routeDataMap[standardizedRouteID] = existingData;
                }
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
        // AnalyzeAverageDelays();
        Debug.Log("debugging counter");
        Debug.Log(counter);
        foreach (var route in routeDataMap)
        {
            Debug.Log(route);
            SpawnMarkAtLatLong(route.Key, route.Value.SourceLatitude, route.Value.SourceLongitude, route.Value.DestLatitude, route.Value.DestLongitude, route.Value.AverageDelay, route.Value.TrafficCount, route.Value.SourceAirport, route.Value.DestAirport, route.Value.SourceCountry, route.Value.DestCountry);
        }
    }

    void AnalyzeAverageDelays()
    {
        int rangeIncrement = 5;
        Dictionary<string, int> delayHistogram = new Dictionary<string, int>();

        foreach (var route in routeDataMap.Values)
        {
            int rangeIndex = (int)(route.AverageDelay / rangeIncrement);
            string rangeKey = $"{rangeIndex * rangeIncrement}-{(rangeIndex + 1) * rangeIncrement}";

            if (!delayHistogram.ContainsKey(rangeKey))
            {
                delayHistogram[rangeKey] = 1;
            }
            else
            {
                delayHistogram[rangeKey]++;
            }
        }

        foreach (var range in delayHistogram)
        {
            Debug.Log($"Average Delay {range.Key}: Count = {range.Value}");
        }
    }

    //  public static void FilterAirportsByAvgDelay(float avgDelay)
    // {
    //     selectedAvgDelay = avgDelay;

    //     foreach (PointScript point in allPointsKd)
    //     {
    //         float pointAvgDelay = (float)point.avgDelay;

    //         point.gameObject.SetActive(
    //             (selectedAvgDelay == -1f || (pointAvgDelay >= selectedAvgDelay && pointAvgDelay <= 100f))
    //         );
    //     }
    // }

    private void updateLocation()
    {
        mapZoom = map.GetComponent<MapRenderer>().ZoomLevel;
        longitudeCenter = map.GetComponent<MapRenderer>().Center.LongitudeInDegrees;
        latitudeCenter = map.GetComponent<MapRenderer>().Center.LatitudeInDegrees;
        xOffSet = CoordinatConverter.NormalizeLongitudeWebMercator(longitudeCenter, mapZoom);
        yOffSet = CoordinatConverter.NormalizeLatitudeWebMercator(latitudeCenter, mapZoom);

        parent.transform.position = new Vector3((float)-xOffSet, (float)-yOffSet, 0.0f);
    }

    public void SpawnMarkAtLatLong(string RouteID, double Source_latitude, double Source_longitude, double Dest_latitude, double Dest_longitude, double avgDelay, double trafficCount, string SourceAirport,string  DestAirport,string  SourceCountry,string DestCountry)
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

