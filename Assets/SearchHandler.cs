// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Maps.Unity;
using Microsoft.Maps.Unity.Search;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(MapRenderer))]
public class SearchHandler : MonoBehaviour
{
    [SerializeField]
    TMP_InputField _inputField = null;
    //dropdown for search results
    public TMP_Dropdown _dropdown = null;


    public async void OnSearch()
    {
        if (MapSession.Current == null || string.IsNullOrWhiteSpace(MapSession.Current.DeveloperKey))
        {
            return;
        }

        var searchText = _inputField.text;
        var result = await MapLocationFinder.FindLocations(searchText);
        if (result.Locations.Count > 0)
        {
            var location = result.Locations[0];
            var mapRenderer = GetComponent<MapRenderer>();
            mapRenderer.SetMapScene(new MapSceneOfLocationAndZoomLevel(location.Point, 6), MapSceneAnimationKind.Bow, 5.0f);
        }
        // we want to display the search results in a dropdown menu and allow the user to select one

        //clear the dropdown
        _dropdown.ClearOptions();
        for (int i = 0; i < result.Locations.Count; i++)
        {
            _dropdown.options.Add(new TMP_Dropdown.OptionData(result.Locations[i].Address.FormattedAddress));
        }
    }

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
    public async void OnDropdownValueChanged()
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
                var mapRenderer = GetComponent<MapRenderer>();
                mapRenderer.SetMapScene(new MapSceneOfLocationAndZoomLevel(location.Point, 6), MapSceneAnimationKind.Bow, 5.0f);
            }
        }

        // clear the dropdown
        _dropdown.ClearOptions();
        _dropdown.value = 0;
        _dropdown.captionText.text = "Select search result...";

        // clear the input field
        _inputField.text = "";


    }
}
