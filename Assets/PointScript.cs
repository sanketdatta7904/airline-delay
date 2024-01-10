using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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

        // change the position of the point
        //transform.localScale = Vector3.one;
        transform.localPosition = new Vector3((float)x, (float)y, 0.0f);
    }
}
