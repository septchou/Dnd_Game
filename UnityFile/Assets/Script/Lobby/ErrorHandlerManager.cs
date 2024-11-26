using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ErrorHandlerManager : MonoBehaviour
{
    public static ErrorHandlerManager Instance;
    [SerializeField] GameObject errorUIPrefab;
    [SerializeField] GameObject currentErrorUI;
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowError(string message)
    {
        if (currentErrorUI == null)
        {

            Canvas canvas = FindObjectOfType<Canvas>();

            if (canvas == null)
            {
                Debug.LogError("No Canvas found in the scene to display error UI!");
                return;
            }

            // Instantiate the error UI in the Canvas
            currentErrorUI = Instantiate(errorUIPrefab, canvas.transform);
            ErrorPanel errorPanel = currentErrorUI.GetComponent<ErrorPanel>();
            TextMeshProUGUI errorText = errorPanel.errorText;
            Button button = errorPanel.closeErrorButton;

            if (errorPanel != null)
            {
                errorText.text = message;
                button.onClick.AddListener(CloseError);
            }
            else
            {
                Debug.LogWarning("Error UI prefab does not have a ErrorPanel component");
            }
        }
    }

    public void CloseError()
    {
        if (currentErrorUI != null)
        {
            Destroy(currentErrorUI);
        }
    }
}
