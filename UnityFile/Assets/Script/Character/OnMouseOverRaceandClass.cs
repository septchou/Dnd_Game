using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class OnMouseOverRaceandClass : MonoBehaviour
{

    [SerializeField] List<GameObject> existListItem = new List<GameObject>();
    [SerializeField] GameObject raceDropdownObj, classDropdownObj, detailListItemPrefab, detailListPanel;
    [SerializeField] TMP_Dropdown raceDropdown, classDropdown;
    [SerializeField] TMP_Text headerText;
    [SerializeField] Transform detailContent;


    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            // Check if the mouse is over the specific dropdown GameObject
            if (RectTransformUtility.RectangleContainsScreenPoint(
                raceDropdownObj.GetComponent<RectTransform>(),
                Input.mousePosition,
                Camera.main))
            {
                popUpRaceDetailUI();
            }

            if (RectTransformUtility.RectangleContainsScreenPoint(
                classDropdownObj.GetComponent<RectTransform>(),
                Input.mousePosition,
                Camera.main))
            {

            }
            else
            {
                detailListPanel.SetActive(false);
            }
        }
    }

    private void popUpRaceDetailUI()
    {
        foreach (GameObject item in existListItem)
        {
            Destroy(item);
        }
        existListItem = new List<GameObject>();

        
        Race race = Resources.Load<Race>("Races/" + raceDropdown.value);
        if(race != null)
        {
            headerText.text = race.name;
            foreach (Skill skill in race.raceSkills)
            {
                GameObject item = Instantiate(detailListItemPrefab, detailContent);
                existListItem.Add(item);
                item.GetComponent<SkillListItem>().SetUp(race.name, skill.detail);
            }

            RectTransform prefabRect = detailListItemPrefab.GetComponent<RectTransform>();
            float prefabHeight = prefabRect.rect.height;
            float totalHeight = prefabHeight * existListItem.Count;
            RectTransform panelRect = detailListPanel.GetComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, totalHeight);
        }
        detailListPanel.SetActive(true);
    }
}
