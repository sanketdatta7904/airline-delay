using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Microsoft.Maps.Unity;

public class MarkSpawner : MonoBehaviour
{
    public GameObject mark;
    public GameObject parent;
    private double longituteCenter;
    private double latitudeCenter;
    private float mapZoom = 1.0f;

    private double xOffSet = 0.0f;
    private double yOffSet = 0.0f;

    public GameObject map;
    // Start is called before the first frame update
    void Start()
    {
        // get the map zoom
        mapZoom = map.GetComponent<MapRenderer>().ZoomLevel;
        longituteCenter = map.GetComponent<MapRenderer>().Center.LongitudeInDegrees;
        latitudeCenter = map.GetComponent<MapRenderer>().Center.LatitudeInDegrees;
        xOffSet = NormalizeLongitudeWebMercator(longituteCenter, mapZoom);
        yOffSet = NormalizeLatitudeWebMercator(latitudeCenter, mapZoom);




        Debug.Log("mapZoom: " + mapZoom);
        Debug.Log("longituteCenter: " + longituteCenter);
        Debug.Log("latitudeCenter: " + latitudeCenter);
        Debug.Log("xOffSet: " + xOffSet);
        Debug.Log("yOffSet: " + yOffSet);

        
        // Charles de Gaulle Airport 49.0096906 2.5479245
        spawnMarkAtLatLong(49.0096906, 2.5479245, mapZoom);
        // Orly Airport 48.7262433 2.3652472
        spawnMarkAtLatLong(48.7262433, 2.3652472, mapZoom);
        // Lyon-Saint Exupéry Airport 45.720362 5.079507
        spawnMarkAtLatLong(45.720362, 5.079507, mapZoom);
        //Toulouse-Blagnac Airport 43.6293863 1.367682
        spawnMarkAtLatLong(43.6293863, 1.367682, mapZoom);
        // Paris Airport-Le Bourget 48.9614725 2.437202
        spawnMarkAtLatLong(48.9614725, 2.437202, mapZoom);
        //leonardo da vinci airport: 41.7999° N, 12.2462° E
        spawnMarkAtLatLong(41.7999, 12.2462, mapZoom);
        // Stuttgart 48.77845 9.18001
        spawnMarkAtLatLong(48.77845, 9.18001, mapZoom);
        //Konstanz 47.69009 9.18825
        spawnMarkAtLatLong(47.69009, 9.18825, mapZoom);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // spawn a mark. 

    public void spawnMark(float x, float y)
    {
        Vector3 spawnPosition = new Vector3(x,y,0);
        GameObject markInstance = Instantiate(mark, spawnPosition, Quaternion.identity);
        markInstance.transform.parent = parent.transform;
    }

    // spawn a mark at a given latitude and longitude
    public void spawnMarkAtLatLong(double latitude, double longitude)
    {
        double x = longitude / 360.0;
        double y = NormalizeLatitudeWebMercator(latitude);
        spawnMark((float)x, (float)y);

    }

    public void spawnMarkAtLatLong(double latitude, double longitude, double zoom)
    {
        double x = NormalizeLongitudeWebMercator(longitude, zoom) - xOffSet;
        double y = NormalizeLatitudeWebMercator(latitude, zoom) - yOffSet;
        spawnMark((float)x, (float)y);

    }
    public static double NormalizeLatitudeWebMercator(double latitude)
    {
        return 0.5 - 0.5* (Math.PI - Math.Log(Math.Tan(Math.PI / 4.0 + (latitude * Math.PI / 180) / 2.0))) / Math.PI;

    }

    public static double NormalizeLatitudeWebMercator(double latitude, double zoom)
    {
        return Math.Pow(2, zoom - 1) * NormalizeLatitudeWebMercator(latitude);
    }

    public static double NormalizeLongitudeWebMercator(double longitude)
    {
        return longitude / 360.0;
    }

    public static double NormalizeLongitudeWebMercator(double longitude, double zoom)
    {
        return Math.Pow(2, zoom - 1) * NormalizeLongitudeWebMercator(longitude);
    }


}
