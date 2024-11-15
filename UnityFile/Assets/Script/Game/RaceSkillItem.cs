using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RaceSkillItem : MonoBehaviour
{
    [SerializeField] TMP_Text skillNameText,skillDetailText;

    public void SetUp(string skillname ,string skilldetail)
    {
        skillNameText.text = skillname;
        skillDetailText.text = skilldetail;
    }
    public void DestroyListItem()
    {
        Destroy(gameObject);
    }
}
