using UnityEngine;
using UnityEngine.UI;

public class HoverScript : MonoBehaviour
{
    public string airportName;
    public string airportCode;
    public double latitude;
    public double longitude;

    // Reference to the UI Text for the tooltip
    public Text tooltipText;

    void Start()
    {
        // Initialize tooltipText by finding the Text component (replace "TooltipText" with your actual Text object name)
        tooltipText = GameObject.Find("TooltipText").GetComponent<Text>();
        tooltipText.gameObject.SetActive(false);
    }

    public void SetupTooltip(string name, string code, double lat, double lon)
    {
        airportName = name;
        airportCode = code;
        latitude = lat;
        longitude = lon;
    }

    void OnMouseEnter()
    {
        // Display the tooltip when the mouse hovers over the mark
        tooltipText.text = $"Airport: {airportName}\nCode: {airportCode}\nLatitude: {latitude}\nLongitude: {longitude}";
        tooltipText.gameObject.SetActive(true);
    }

    void OnMouseExit()
    {
        // Hide the tooltip when the mouse exits the mark
        tooltipText.gameObject.SetActive(false);
    }
}
