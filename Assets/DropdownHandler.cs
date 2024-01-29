using UnityEngine;
using UnityEngine.UI;

public class DropdownHandler : MonoBehaviour
{
    public Dropdown airportTypeDropdown;

    private void Start()
    {
        // Subscribe to the dropdown's value changed event
        airportTypeDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    private void OnDropdownValueChanged(int value)
    {
        // Notify the MarkSpawner about the selected value
        MarkSpawner.FilterAirportsByType(airportTypeDropdown.options[value].text);
    }
}
