using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverloyOverMouse : MonoBehaviour
{
    [SerializeField] bool isOverlayOn = false;

    [SerializeField] GameObject selectedOverlay;

    void Start()
    {

    }

    void Update()
    {
        if (isOverlayOn)
        {
            Vector2 mousePosition = Input.mousePosition;
            selectedOverlay.GetComponent<RectTransform>().position = mousePosition;
            selectedOverlay.SetActive(true);
        }
        
        
    }

    public void CreatedMouseOverlay(GameObject overlay)
    {
        //if (overlay.GetComponent<RectTransform>() == null) return;
        selectedOverlay = overlay;
        isOverlayOn = true;
    }

    public void RemoveOverlay()
    {
        isOverlayOn = false;
        selectedOverlay.SetActive(false);
    }
}
