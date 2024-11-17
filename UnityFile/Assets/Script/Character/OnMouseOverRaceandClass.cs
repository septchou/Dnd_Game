using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using UnityEngine.UI;

public class OnMouseOverRaceandClass : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    [SerializeField] List<GameObject> existListItem = new List<GameObject>();
    [SerializeField] GameObject detailListItemPrefab, raceDetailListPanel, classDetailListPanel;
    [SerializeField] TMP_Text headerText;
    [SerializeField] Transform detailContent;
    [SerializeField] CharacterCreation characterCreation;
    [SerializeField] bool isMouseOver = false;

    void Update()
    {
        if (isMouseOver)
        {
            /*raceDetailListPanel.SetActive(true);
            classDetailListPanel.SetActive(false);*/
        }
        else
        {
            //raceDetailListPanel.SetActive(false);
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        popUpRaceDetailUI();
        isMouseOver = true;
        raceDetailListPanel.SetActive(true);
        classDetailListPanel.SetActive(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //popUpRaceDetailUI();
        isMouseOver = false;
        raceDetailListPanel.SetActive(true);
        classDetailListPanel.SetActive(false);
    }


    private void popUpRaceDetailUI()
    {
        foreach (GameObject item in existListItem)
        {
            Destroy(item);
        }
        existListItem = new List<GameObject>();


        Race race = characterCreation.GetRaceFromDropDown();
        if(race != null)
        {
            //float prefabHeight = 0;
            headerText.text = race.name;
            foreach (Skill skill in race.raceSkills)
            {
                GameObject item = Instantiate(detailListItemPrefab, detailContent);
                existListItem.Add(item);
                LayoutRebuilder.ForceRebuildLayoutImmediate(item.GetComponent<RectTransform>());
                item.GetComponent<SkillListItem>().SetUp(skill.skillName, skill.detail);
                //RectTransform prefabRect = detailListItemPrefab.GetComponent<RectTransform>();
                //prefabHeight += prefabRect.rect.height;
            }

            /*float totalHeight = prefabHeight + 60;
            RectTransform panelRect = raceDetailListPanel.GetComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, totalHeight);*/
        }

    }

}
