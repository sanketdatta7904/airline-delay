using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TooltipBehavior : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private GameObject tooltip;
    private RectTransform chartContainer; // New variable to store the chartContainer reference

    public void SetTooltipText(GameObject tooltipObject)
    {
        tooltip = tooltipObject;
    }

    // New method to set the chartContainer reference
    public void SetChartContainer(RectTransform container)
    {
        chartContainer = container;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltip != null)
            tooltip.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltip != null)
            tooltip.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Clicked on the bar!");
    }
}
