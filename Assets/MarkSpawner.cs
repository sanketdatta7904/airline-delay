using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MarkSpawner : MonoBehaviour
{
    public GameObject mark;
    public GameObject parent;
    public float longituteCenter;
    public float latitudeCenter;
    public float mapZoom;
    // Start is called before the first frame update
    void Start()
    {
        //spawnMark(1, 0);

        //spawnMarkAtLatLong(48.864716f, 2.349014f);
        // coordinates of Paris: 48.864716, 2.349014
        double latitude = 48.864716;
        double longitude = 2.349014;

        double x = longitude / 360.0;
        double y = NormalizeLatitudeWebMercator(latitude);
        Debug.Log("x: " + x + " y: " + y);

        spawnMark((float)x, (float)y);

        // iceland: 64.9631° N, -19.0208° W
        spawnMarkAtLatLong(64.9631, -19.0208);
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
    public static double NormalizeLatitudeWebMercator(double latitude)
    {
        return 0.5 - 0.5* (Math.PI - Math.Log(Math.Tan(Math.PI / 4.0 + (latitude * Math.PI / 180) / 2.0))) / Math.PI;

    }


}
