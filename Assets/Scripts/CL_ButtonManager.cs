using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;

public class CL_ButtonManager : MonoBehaviour
{
    public static CL_ButtonManager Instance;
    public GameObject menuOverlay;
    public Image previewImage;
    public TMP_InputField nameInput;

    public List<LevelIconData> icons = new List<LevelIconData>();
    public string levelName;

    private LevelIconData currentData;
    private MenuLevelButton currentButton;

    private void Awake()
    {
        Instance = this;
        menuOverlay.SetActive(false);
    }

    private void Start()
    {
        LevelManager.Instance.LoadAllLevelButtons();
    }

    public void OpenMenu(LevelIconData data, MenuLevelButton button)
    {
        currentData = data;
        currentButton = button;

        menuOverlay.SetActive(true);
        nameInput.text = data.iconName;
    }

    public void CloseMenu()
    {
        menuOverlay.SetActive(false);
    }

    public void RenameLevel()
    {
        currentData.iconName = nameInput.text;
        currentButton.UpdateVisuals();
        LevelManager.Instance.SaveLevelIcon(currentData);
    }
    public void DeleteLevel()
    {
        // 1. Remove from list
        icons.Remove(currentData);

        // 2. Delete the file
        LevelManager.Instance.DeleteLevel(currentData);

        // 3. Destroy the UI button
        Destroy(currentButton.gameObject);

        // 4. Free the placeholder slot
        CustomLevelManager.Instance.FreeSlot(currentData.placeholderIndex);
        CloseMenu();
        //Debug.Log("Deleted level: " + currentData.iconName);
    }

    public void PlayLevel()
    {
        LevelManager.Instance.OpenLevel(currentData);
    }

    public void ChangeImage(Sprite newSprite)
    {
        //currentData.icon = newSprite;

        //previewImage.sprite = newSprite;
        currentButton.UpdateVisuals();
    }
    public void RebuildButtonsFromData()
    {
        /*foreach (Transform child in CustomLevelManager.Instance.iconParent)
            Destroy(child.gameObject);*/

        foreach (var icon in icons)
            CustomLevelManager.Instance.CreateIconFromData(icon);
    }
}