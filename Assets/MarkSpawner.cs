using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Microsoft.Maps.Unity;
using Unity.VisualScripting;

public class MarkSpawner : MonoBehaviour
{
    public GameObject mark;
    public GameObject parent;
    public GameObject[] allPoints;
    private double longituteCenter;
    private double latitudeCenter;
    public static float mapZoom = 1.0f;

    private double xOffSet = 0.0f;
    private double yOffSet = 0.0f;

    public GameObject map;
    // Start is called before the first frame update
    void Start()
    {
        // set the variables for the map
        updateLocation();

        
        // Charles de Gaulle Airport 49.0096906 2.5479245
        SpawnMarkAtLatLong(49.0096906, 2.5479245, mapZoom);
        // Orly Airport 48.7262433 2.3652472
        SpawnMarkAtLatLong(48.7262433, 2.3652472, mapZoom);
        // Lyon-Saint Exupéry Airport 45.720362 5.079507
        SpawnMarkAtLatLong(45.720362, 5.079507, mapZoom);
        //Toulouse-Blagnac Airport 43.6293863 1.367682
        SpawnMarkAtLatLong(43.6293863, 1.367682, mapZoom);
        // Paris Airport-Le Bourget 48.9614725 2.437202
        SpawnMarkAtLatLong(48.9614725, 2.437202, mapZoom);
        //leonardo da vinci airport: 41.7999° N, 12.2462° E
        SpawnMarkAtLatLong(41.7999, 12.2462, mapZoom);
        // Stuttgart 48.77845 9.18001
        SpawnMarkAtLatLong(48.77845, 9.18001, mapZoom);
        //Konstanz 47.69009 9.18825
        SpawnMarkAtLatLong(47.69009, 9.18825, mapZoom);

    }

    // Update is called once per frame
    void Update()
    {
        // check if the map has changed
        if (mapZoom != map.GetComponent<MapRenderer>().ZoomLevel || longituteCenter != map.GetComponent<MapRenderer>().Center.LongitudeInDegrees || latitudeCenter != map.GetComponent<MapRenderer>().Center.LatitudeInDegrees)
        {
            updateLocation();
           


            // call the Redraw function on each point
            foreach (GameObject point in allPoints)
            {
                point.GetComponent<PointScript>().Redraw(mapZoom);
            }
        }
    }

    private void updateLocation()
    {
        mapZoom = map.GetComponent<MapRenderer>().ZoomLevel;
        longituteCenter = map.GetComponent<MapRenderer>().Center.LongitudeInDegrees;
        latitudeCenter = map.GetComponent<MapRenderer>().Center.LatitudeInDegrees;
        xOffSet = CoordinatConverter.NormalizeLongitudeWebMercator(longituteCenter, mapZoom);
        yOffSet = CoordinatConverter.NormalizeLatitudeWebMercator(latitudeCenter, mapZoom);

        // change the position of the parent element based on the x and y offset
        parent.transform.position = new Vector3((float)-xOffSet, (float)-yOffSet, 0.0f);
    }

    // spawn a mark. 
    public void SpawnMarkAtLatLong(double latitude, double longitude, double zoom)
    {
        float x = (float)CoordinatConverter.NormalizeLongitudeWebMercator(longitude, zoom);
        float y = (float)CoordinatConverter.NormalizeLatitudeWebMercator(latitude, zoom);

        // spawn the mark in the middle of the map
        GameObject markInstance = Instantiate(mark, Vector3.zero, Quaternion.identity);

        // set the position of the mark
        markInstance.transform.parent = parent.transform; // set the parent of the mark to the parent object
        markInstance.GetComponent<PointScript>().latitude = latitude;
        markInstance.GetComponent<PointScript>().longitude = longitude;
        markInstance.GetComponent<PointScript>().Redraw(mapZoom);
        
        // add to list allPoints for future reference
        Array.Resize(ref allPoints, allPoints.Length + 1);
        allPoints[allPoints.Length - 1] = markInstance;
    }

}
