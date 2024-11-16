using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SkillListItem : MonoBehaviour
{
    [SerializeField] TMP_Text skillNameText, skillDetailText;
    [SerializeField] RectTransform rectTransform;
    [SerializeField] RectTransform skillDetailTextRectTransform;
    public void SetUp(string skillName, string skillDetail)
    {
        // Set the text
        skillNameText.text = skillName;
        skillDetailText.text = skillDetail;

        // Force TextMeshPro to update and calculate preferred size
        skillNameText.ForceMeshUpdate();
        skillDetailText.ForceMeshUpdate();

        // Get the preferred height of the text
        float skillNameHeight = skillNameText.preferredHeight;
        float skillDetailHeight = skillDetailText.preferredHeight;

        Debug.Log("Skill Name Height: " + skillNameHeight);
        Debug.Log("Skill Detail Height: " + skillDetailHeight);

        // Adjust the height of skillDetailTextGameObj based on the text
        if (skillDetailTextRectTransform != null)
        {
            Vector2 detailSize = skillDetailTextRectTransform.sizeDelta;
            skillDetailTextRectTransform.sizeDelta = new Vector2(detailSize.x, skillDetailHeight);
        }

        // Adjust the height of the main GameObject based on the total text heights
        if (rectTransform != null)
        {
            Vector2 newSize = rectTransform.sizeDelta;
            rectTransform.sizeDelta = new Vector2(newSize.x, skillNameHeight + skillDetailHeight + 20); // Add padding
        }
    }

}
