using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugConsole : MonoBehaviour
{
    public static DebugConsole instance;

    [SerializeField] RectTransform displayRect;
    [SerializeField] Text displayText;

    float initHright;
    void Awake()
    {
        if(DebugConsole.instance != null)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            DebugConsole.instance = this;
        }

        initHright = displayRect.anchoredPosition.y;
    }

    public void ChangeDisplayPoistion(float newPos)
    {
        displayRect.anchoredPosition = new Vector2(displayRect.anchoredPosition.x,initHright+newPos);
    }

    public void Log(string newLog)
    {
        displayText.text = newLog + "\n" + displayText.text;
    }
}
