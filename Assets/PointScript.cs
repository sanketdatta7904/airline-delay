using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PointScript : MonoBehaviour
{
    // coordinates of the point
    public double latitude = 0.0;
    public double longitude = 0.0;
    // information about the point
    public string airportName = "";
    public string airportCode = "";
    public string size = ""; // "small", "medium", "large"
    public double avgDelay = 0.0; // average delay of the airport
    public bool hidePointsOffMap = false;

    private double maxX = 1.3;
    private double maxY = 0.51;
    private double minX = -1.3;
    private double minY = -0.51;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }



    void OnMouseEnter()
    {
        Debug.Log("Mouse enter");
        // Display the tooltip when the mouse hovers over the mark
        //tooltipText.text = $"Airport: {airportName}\nCode: {airportCode}\nLatitude: {latitude}\nLongitude: {longitude}";
        //tooltipText.gameObject.SetActive(true);
    }

    void OnMouseExit()
    {
        Console.WriteLine("Mouse exit");
        // Hide the tooltip when the mouse exits the mark
        //tooltipText.gameObject.SetActive(false);
    }

    public void Redraw(float zoom)
    {
        // depending on the map zoom, we dont want to see all the points
        // if the zoom is <4, we only want to see the large points
        // if the zoom is <5, we only want to see the medium and large points
        // else we want to see all the points
        if (zoom < 4 && size != "large")
        {
            // disable the sprite renderer
            GetComponent<SpriteRenderer>().enabled = false;
        }
        else if (zoom < 5 && size == "small")
        {
            // disable the sprite renderer
            GetComponent<SpriteRenderer>().enabled = false;
        }
        else
        {
            // enable the sprite renderer
            GetComponent<SpriteRenderer>().enabled = true;
        }

        // decrease the size of the point depending on the zoom
        //transform.localScale = new Vector3(1.0f / zoom, 1.0f / zoom, 1.0f / zoom);

        double x = CoordinatConverter.NormalizeLongitudeWebMercator(longitude, zoom);
        double y = CoordinatConverter.NormalizeLatitudeWebMercator(latitude, zoom);

        // check if the point is out of bounds. If it is, save performance by disabling the sprite renderer
        // additionally we want to disable the rigidbody and collider
        if (hidePointsOffMap && (x > maxX || x < minX || y > maxY || y < minY))
        {
            // disable the sprite renderer, rigidbody and collider
            GetComponent<SpriteRenderer>().enabled = false;
            //GetComponent<Rigidbody2D>().simulated = false;
            //GetComponent<CircleCollider2D>().enabled = false;
        }
        else
        {
            // enable the sprite renderer, rigidbody and collider
            GetComponent<SpriteRenderer>().enabled = true;
            //GetComponent<Rigidbody2D>().simulated = true;
            //GetComponent<CircleCollider2D>().enabled = true;

            // change the position of the point
            transform.localPosition = new Vector3((float)x, (float)y, 0.0f);
        }

        
    }
}
