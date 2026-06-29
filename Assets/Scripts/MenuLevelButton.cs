using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuLevelButton : MonoBehaviour
{
    public LevelIconData data;
    public TMP_Text nameText;

    public void Setup(LevelIconData newData)
    {
        data = newData;
        int a = data.placeholderIndex + 1;
        nameText.text = data.iconName;
        data.iconName = nameText.text.ToString();
    }

    public void OnClick()
    {
        CL_ButtonManager.Instance.OpenMenu(data, this);
    }

    public void UpdateVisuals()
    {
        nameText.text = data.iconName;
    }
}