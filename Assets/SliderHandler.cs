using UnityEngine;
using UnityEngine.UI;

public class SliderHandler : MonoBehaviour
{
    public Slider avgDelaySlider;
    public Text sliderNumber; // Reference to the Text element

    private void Start()
    {
        // Subscribe to the slider's value changed event
        avgDelaySlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void OnSliderValueChanged(float value)
    {
        // Update the text to display the current slider value
        sliderNumber.text = value.ToString("F2"); // "F2" formats the float to two decimal places

        // Notify the MarkSpawner about the selected average delay range
        MarkSpawner.FilterAirportsByAvgDelay(value);
    }
}
