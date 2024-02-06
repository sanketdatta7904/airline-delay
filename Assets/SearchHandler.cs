// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections;
using Microsoft.Maps.Unity;
using Microsoft.Maps.Unity.Search;
using TMPro;
using UnityEditor.VersionControl;
using UnityEngine;

[RequireComponent(typeof(MapRenderer))]
public class SearchHandler : MonoBehaviour
{
    [SerializeField]
    TMP_InputField _inputField = null;
    //dropdown for search results
    public TMP_Dropdown _dropdown = null;
    public GameObject markLayer;
    public GameObject highlightPrefab;

    // this method sets the dropdown values based on the entered search text
    public async void OnSearchDropdown()
    {
        if (MapSession.Current == null || string.IsNullOrWhiteSpace(MapSession.Current.DeveloperKey))
        {
            return;
        }

        var searchText = _inputField.text;
        var result = await MapLocationFinder.FindLocations(searchText);
        if (result.Locations.Count > 0)
        {
            // we want to display the search results in a dropdown menu and allow the user to select one
            //clear the dropdown
            _dropdown.ClearOptions();
            for (int i = 0; i < result.Locations.Count; i++)
            {
                Debug.Log(result.Locations[i].Address.FormattedAddress);
                _dropdown.options.Add(new TMP_Dropdown.OptionData(result.Locations[i].Address.FormattedAddress));
            }

            // set the first value as the selected value
            _dropdown.value = 0;
            // set the label of the dropdown to the selected value
            _dropdown.captionText.text = _dropdown.options[_dropdown.value].text;
        }
        
    }   

    // get the selected value from the dropdown and set the map scene to the selected location
    public async void OnSearch()
    {
        if (MapSession.Current == null || string.IsNullOrWhiteSpace(MapSession.Current.DeveloperKey))
        {
            return;
        }

        var searchText = _dropdown.options[_dropdown.value].text;
        if(searchText != null)
        {
            var result = await MapLocationFinder.FindLocations(searchText);
            if (result.Locations.Count > 0)
            {
                var location = result.Locations[0];

                // get tge position of the selected location
                var latitude = location.Point.LatitudeInDegrees;
                var longitude = location.Point.LongitudeInDegrees;

                // get the closest airport to the selected location
                //HighlightClosestAirport(latitude, longitude);

                var mapRenderer = GetComponent<MapRenderer>();
                // A yieldable object is returned that can be used to wait for the end of the animation in a coroutine.

                 mapRenderer.SetMapScene(new MapSceneOfLocationAndZoomLevel(location.Point, 6), MapSceneAnimationKind.Bow, 5.0f);

                // wait until the map scene is set
                StartCoroutine(AnimateMapAndHighlightLocation(mapRenderer, location));

            }
        }

        // clear the dropdown
        _dropdown.ClearOptions();
        _dropdown.value = 0;
        _dropdown.captionText.text = "Select search result...";

        // clear the input field
        _inputField.text = "";


    }

    private IEnumerator AnimateMapAndHighlightLocation(MapRenderer mapRenderer, MapLocation location)
    {
        // Start the animation
        var animation = mapRenderer.SetMapScene(new MapSceneOfLocationAndZoomLevel(location.Point, 6), MapSceneAnimationKind.Bow, 5.0f);

        // Wait for the animation to complete
        yield return animation;

        // Now you can execute more code after the animation is finished
        Debug.Log("Animation is complete!");

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        PointScript nearestObj = MarkSpawner.getClosestPoint(worldPos);
        Debug.Log("nearest airport: " + nearestObj.airportName);

        float distance = Vector3.Distance(nearestObj.transform.position, worldPos);

        string message = "Airport Name: " + nearestObj.airportName + "\n" +
                         "Airport Code: " + nearestObj.airportCode + "\n" +
                         "Average Delay: " + nearestObj.avgDelay + "\n" +
                         "Distance: " + distance + "\n" +
                         "Type: " + nearestObj.airportType;

        Debug.Log(distance);

        if (distance < 1.0005)
        {
            // Show tooltip
            TooltipManager._instance.SetAndShowTooltip(message);
        }
        else
        {
            // Hide tooltip
            TooltipManager._instance.HideTooltip();
        }

        // highlight the nearestobj by changing the color to black
        nearestObj.GetComponent<SpriteRenderer>().color = Color.black;

    }
}
