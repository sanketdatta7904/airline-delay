using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoordinatConverter : MonoBehaviour
{
    private static double NormalizeLatitudeWebMercator(double latitude)
    {
        return 0.5 - 0.5 * (Math.PI - Math.Log(Math.Tan(Math.PI / 4.0 + (latitude * Math.PI / 180) / 2.0))) / Math.PI;

    }

    public static double NormalizeLatitudeWebMercator(double latitude, double zoom)
    {
        return Math.Pow(2, zoom - 1) * NormalizeLatitudeWebMercator(latitude);
    }

    private static double NormalizeLongitudeWebMercator(double longitude)
    {
        return longitude / 360.0;
    }

    public static double NormalizeLongitudeWebMercator(double longitude, double zoom)
    {
        return Math.Pow(2, zoom - 1) * NormalizeLongitudeWebMercator(longitude);
    }
}
