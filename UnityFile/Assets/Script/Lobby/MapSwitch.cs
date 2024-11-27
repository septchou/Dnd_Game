using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapSwitch : MonoBehaviour
{
    public static MapSwitch Instance;
    public bool isSwitchMap;
    public Button switchMapButton;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject); 
        }

        isSwitchMap = false;

        

    }


    public void ChangeToMapScene(string sceneName)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.AutomaticallySyncScene = false;
            Hashtable customProperties = new Hashtable
            {
                { "MasterScene", sceneName }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);


            isSwitchMap = true;

            SceneManager.LoadScene(sceneName);
            MenuManager.Instance.OpenMenu("loading");
        }
        else
        {

        }
    }

    public void ReassignButton()
    {
        GameObject buttonObject = GameObject.Find("MapButton");
        if (buttonObject != null)
        {
            switchMapButton = buttonObject.GetComponent<Button>();
            switchMapButton.onClick.AddListener(() => ChangeToMapScene("MapMaker"));
        }
        else
        {
            Debug.LogWarning("Button not found in the new scene.");
        }
    }

}
