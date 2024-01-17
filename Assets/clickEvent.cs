using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class clickEvent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // mouse click listener
    private void OnMouseDown()
    {
        Debug.Log("Mouse down");
        // mouse position
        Vector3 mousePos = Input.mousePosition;
        // convert mouse position to world position
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

        // get the closest object
        MarkSpawner.getClosestPoint(worldPos);

    }
}
