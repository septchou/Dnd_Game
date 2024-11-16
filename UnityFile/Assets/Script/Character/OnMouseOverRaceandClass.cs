using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class OnMouseOverRaceandClass : MonoBehaviour
{

    public GameObject raceDropdown; // Assign the dropdown GameObject in the inspector

    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            // Check if the mouse is over the specific dropdown GameObject
            if (RectTransformUtility.RectangleContainsScreenPoint(
                raceDropdown.GetComponent<RectTransform>(),
                Input.mousePosition,
                Camera.main))
            {
                
            }
        }
    }
}
