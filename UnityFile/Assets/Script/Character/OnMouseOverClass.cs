using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OnMouseOverClass : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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
            /*raceDetailListPanel.SetActive(false);
            classDetailListPanel.SetActive(true);*/
        }
        else
        {
            //classDetailListPanel.SetActive(false);
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        popUpClassDetailUI();
        isMouseOver = true;
        raceDetailListPanel.SetActive(false);
        classDetailListPanel.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        popUpClassDetailUI();
        isMouseOver = false;
        raceDetailListPanel.SetActive(false);
        classDetailListPanel.SetActive(true);
    }

    private void popUpClassDetailUI()
    {
        foreach (GameObject item in existListItem)
        {
            Destroy(item);
        }
        existListItem = new List<GameObject>();

        CharacterClass characterclass = characterCreation.GetCharacterClassFromDropDown();
        if (characterclass != null)
        {
            headerText.text = characterclass.name;
            foreach (Skill skill in characterclass.classSkills)
            {
                GameObject item = Instantiate(detailListItemPrefab, detailContent);
                LayoutRebuilder.ForceRebuildLayoutImmediate(item.GetComponent<RectTransform>());
                existListItem.Add(item);
                item.GetComponent<SkillListItem>().SetUp(skill.skillName, skill.detail);
            }

            /*RectTransform prefabRect = detailListItemPrefab.GetComponent<RectTransform>();
            float prefabHeight = prefabRect.rect.height;
            float totalHeight = prefabHeight * existListItem.Count + 260;
            RectTransform panelRect = classDetailListPanel.GetComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, totalHeight);*/
        }

    }

}
