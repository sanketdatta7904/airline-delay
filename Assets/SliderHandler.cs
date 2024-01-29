using UnityEngine;
using UnityEngine.UI;

public class SliderHandler : MonoBehaviour
{
    public Slider avgDelaySlider;

    private void Start()
    {
        // Subscribe to the slider's value changed event
        avgDelaySlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void OnSliderValueChanged(float value)
    {
        // Notify the MarkSpawner about the selected average delay range
        MarkSpawner.FilterAirportsByAvgDelay(value);
    }
}
