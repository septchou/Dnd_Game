using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Message : MonoBehaviour
{
    public TMP_Text myMessage;  // Reference to the TMP_Text component
    public Image backgroundImage;  // Reference to the Image component (background)

    void Start()
    {
        // Ensure the background image is behind the text
        GetComponent<RectTransform>().SetAsFirstSibling();

    }

    public void ResizeBackgroundToFitText()
    {
        // Wait until the text is fully laid out before resizing
        Canvas.ForceUpdateCanvases();

        // Get the preferred height of the text for the fixed width
        float fixedWidth = backgroundImage.GetComponent<RectTransform>().sizeDelta.x;
        Vector2 textSize = myMessage.GetPreferredValues(fixedWidth, 0); // Calculate height for fixed width

        // Set only the height of the background image to fit the text, keeping the width fixed
        RectTransform backgroundRect = backgroundImage.GetComponent<RectTransform>();

        // Optional: Add padding to the top and bottom (e.g., 10 units on each side)
        float padding = 20f;
        backgroundRect.sizeDelta = new Vector2(fixedWidth, textSize.y + padding * 2);
    }

}
