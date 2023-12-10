using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointScript : MonoBehaviour
{
    // coordinates of the point
    public double latitude = 0.0;
    public double longitude = 0.0;
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
        double x = CoordinatConverter.NormalizeLongitudeWebMercator(longitude, zoom);
        double y = CoordinatConverter.NormalizeLatitudeWebMercator(latitude, zoom);

        // change the position of the point
        //transform.localScale = Vector3.one;
        transform.localPosition = new Vector3((float)x, (float)y, 0.0f);
    }
}
