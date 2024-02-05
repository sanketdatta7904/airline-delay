using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager _instance;

    public TextMeshProUGUI textComponent;
    private void Awake()
    {
        if (_instance!=null && _instance!=this)
        {
            Destroy(this.gameObject);
            // ensure that there is only one instance of this object
        }
        else
        {
            _instance = this;
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = true;
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // this sets the middle of the tooltip to the mouse position
        // we wamt to put the bootom left corner of the tooltip to the mouse position
        // so we need to add an offset
        //transform.position = Input.mousePosition;
        //double xOffset = 250;
        //double yOffset = 110;
        //transform.position = new Vector3((float)(transform.position.x + xOffset), (float)(transform.position.y + yOffset), transform.position.z);
    }

    public void SetAndShowTooltip(string text)
    {
        textComponent.text = text;
        gameObject.SetActive(true);
    }

    public void HideTooltip()
    {
        gameObject.SetActive(false);
        textComponent.text = string.Empty;
    }

    public void ToggleTooltip()
    {
        if (textComponent.text != string.Empty)
        {
            TooltipManager._instance.HideTooltip();
        }
    }
}
