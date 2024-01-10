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
        transform.position = Input.mousePosition;
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
}
