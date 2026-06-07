using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlaceButton : MonoBehaviour
{
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private int itemPrefabIndex;
    [SerializeField] private RectTransform myButtonRect;

    public void IndexPlacing()
    {
        if (gameObject.CompareTag("BirdButton"))
        {
            //Debug.Log("Bird button clicked");
            levelManager.PlaceBirdInLocation(itemPrefabIndex);
        }
        else
        {
            levelManager.PlaceObject(itemPrefabIndex, myButtonRect);
        }
    }
}