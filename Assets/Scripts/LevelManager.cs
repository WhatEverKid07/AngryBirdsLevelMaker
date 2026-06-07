using DG.Tweening;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [SerializeField] private Camera worldCamera;  // Camera rendering your 3D world
    [SerializeField] private Canvas canvas;       // UI canvas
    [SerializeField] private PathPoints pathPoints;

    private Score score;

    [Header("Available Objects to Place")]
    [SerializeField] private GameObject[] placeablePrefabs;
    [SerializeField] private GameObject[] birdPlaceLocation;

    [Header("Save Settings")]
    private string saveFileFolderName = "saves";
    private string savedLevelJson = null;

    private List<GameObject> placedObjects = new List<GameObject>();

    [Header("Play game settings")]
    [SerializeField] GameManager gameManager;
    [SerializeField] SlingShot slingShot;

    // For Birds
    private Bird bird;
    private List<Bird> birdScript = new List<Bird>();
    private int birdOrder = 1;

    // For Bricks
    private BrickManager brickManager;
    private List<BrickManager> brickManagerScripts = new List<BrickManager>();

    // For Pigs
    private PigManager pigManager;
    private List<PigManager> pigManagerScripts = new List<PigManager>();

    private BounceManager bounceManager;
    private List<BounceManager> bounceManagerScripts = new List<BounceManager>();

    private ObjectMovRot objectMovRot;
    [SerializeField] private GameObject playGame;
    [SerializeField] private GameObject buildGame;

    [SerializeField] private bool normalLevel = false;
    [SerializeField] private bool startMenu= false;

    [SerializeField] public static LevelIconData iconDataLM;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        if (startMenu) { return; }
        score = GameObject.Find("number").GetComponent<Score>();
        LoadLevelObjects(iconDataLM);
        BuildGame();
        if (!normalLevel) { return; }

        gameManager.GameManager_Activate();
        slingShot.SlingShot_Activate();
        foreach (BrickManager brickScript in brickManagerScripts)
        {
            //brickScript.SetLocation();
            brickScript.buildMode = !brickScript.buildMode;
        }
        foreach (PigManager pigScript in pigManagerScripts)
        {
            //pigScript.SetLocation();
            //pigScript.buildMode = !pigScript.buildMode;
            pigScript.buildMode = false;
            Debug.Log("Pig.Buildmode switch on: " + pigScript.gameObject.name);
        }
        foreach (Bird birdScript in birdScript)
        {
            if (birdScript != null)
            {
                birdScript.Bird_Activate();
            }
        }
        Debug.Log("Normal level start.");
    }

    public void PlaceBirdInLocation(int prefabIndex)
    {
        if (prefabIndex < 0 || prefabIndex >= placeablePrefabs.Length) return;

        for (int i = 0; i < birdPlaceLocation.Length; i++)
        {
            //Vector3 spawnPos = birdPlaceLocation[i].transform.position;

            Transform slot = birdPlaceLocation[i].transform;

            if (slot.childCount == 0)
            {
                Vector3 spawnPos = slot.position;

                GameObject obj = Instantiate(placeablePrefabs[prefabIndex], spawnPos, Quaternion.identity);
                obj.transform.localScale = Vector3.one * 0.25f;

                obj.transform.SetParent(slot);

                placedObjects.Add(obj);
                GetPlacedObjScripts();

                var mov = obj.GetComponent<ObjectMovRot>();
                mov.isPlacing = false;
                mov.canMove = false;

                var bird = obj.GetComponent<Bird>();
                if (bird != null)
                {
                    bird.birdOrder = birdOrder;
                    birdOrder++;
                }

                // IMPORTANT: stop after placing ONE bird
                break;
            }
        }
    }

    public void PlaceObject(int prefabIndex, RectTransform uiButtonRect)
    {
        if (prefabIndex < 0 || prefabIndex >= placeablePrefabs.Length) return;

        Vector3 worldPos;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
            uiButtonRect,
            RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, uiButtonRect.position),
            worldCamera,
            out worldPos))
        {
            Vector3 spawnPos = worldPos + Vector3.up;
            spawnPos.z = 0f;

            GameObject obj = Instantiate(placeablePrefabs[prefabIndex], spawnPos, Quaternion.identity);
            obj.transform.localScale = Vector3.one * 0.25f;
            placedObjects.Add(obj);
            GetPlacedObjScripts();
            objectMovRot = obj.GetComponent<ObjectMovRot>();
            objectMovRot.isPlacing = true;
            objectMovRot = null;

            var bird = obj.GetComponent<Bird>();
            if (bird != null)
            {
                bird.birdOrder = birdOrder;
                birdOrder++;
                bird = null;
            }
        }
    }

    public void RemoveObject(GameObject obj)
    {
        if (placedObjects.Contains(obj))
        {
            placedObjects.Remove(obj);
            Destroy(obj);
            if (obj.GetComponent<Bird>() != null)
            {
                birdOrder--;
            }
            GetPlacedObjScripts();
        }
    }

    public void GetPlacedObjScripts()
    {
        bounceManagerScripts.Clear();
        brickManagerScripts.Clear();
        pigManagerScripts.Clear();
        birdScript.Clear();

        foreach (var obj in placedObjects)
        {
            if (obj == null) continue;

            var brickManager = obj.GetComponent<BrickManager>();
            if (brickManager != null)
                brickManagerScripts.Add(brickManager);

            var pigManager = obj.GetComponent<PigManager>();
            if (pigManager != null)
                pigManagerScripts.Add(pigManager);

            var bird = obj.GetComponent<Bird>();
            if (bird != null)
                birdScript.Add(bird);
            
            var bounceScript = obj.GetComponent<BounceManager>();
            if (bounceScript != null)
                bounceManagerScripts.Add(bounceScript);
        }
    }

    public void SaveLevelObjects()
    {
        if (iconDataLM == null)
        {
            Debug.LogWarning("No active iconDataLM set for saving objects.");
            return;
        }

        //string folder = Path.Combine(Application.persistentDataPath, saveFileFolderName);
        string folder = Path.Combine(Directory.GetParent(Application.dataPath).FullName, saveFileFolderName);
        Directory.CreateDirectory(folder);

        string filePath = Path.Combine(folder, $"{iconDataLM.levelID}.json");

        LevelData data;

        // Load existing file if it exists
        if (File.Exists(filePath))
        {
            string existingJson = File.ReadAllText(filePath);
            data = JsonUtility.FromJson<LevelData>(existingJson);
        }
        else
        {
            data = new LevelData();
        }

        // Replace objects with current placedObjects
        data.objects.Clear();

        foreach (var obj in placedObjects)
        {
            LevelObjectData objData = new LevelObjectData();
            objData.prefabName = obj.name.Replace("(Clone)", "").Trim();
            objData.position = obj.transform.position;
            objData.rotation = obj.transform.rotation;

            Bird bird = obj.GetComponent<Bird>();
            if (bird != null)
            {
                objData.birdOrder = bird.birdOrder;
            }

            data.objects.Add(objData);
        }

        // Ensure icon info is up to date
        data.levelName = iconDataLM.iconName;
        data.icons.Clear();
        data.icons.Add(iconDataLM);

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);

        Debug.Log("Saved level objects to: " + filePath);
    }



    public void SaveLevelIcon(LevelIconData icon)
    {
        //string folder = Path.Combine(Application.persistentDataPath, saveFileFolderName);
        string folder = Path.Combine(Directory.GetParent(Application.dataPath).FullName, saveFileFolderName);
        Directory.CreateDirectory(folder);

        string filePath = Path.Combine(folder, $"{icon.levelID}.json");

        LevelData data;

        if (File.Exists(filePath))
        {
            string existingJson = File.ReadAllText(filePath);
            data = JsonUtility.FromJson<LevelData>(existingJson);
        }
        else
        {
            data = new LevelData();
        }

        data.levelName = icon.iconName;
        data.icons.Clear();
        data.icons.Add(icon);

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);

        Debug.Log("Saved level icon to: " + filePath);
    }

    public void LoadAllLevelButtons()
    {
        string folder = Path.Combine(Directory.GetParent(Application.dataPath).FullName, saveFileFolderName);
        Directory.CreateDirectory(folder);

        string[] files = Directory.GetFiles(folder, "*.json");

        CL_ButtonManager.Instance.icons.Clear();

        // Track used slots
        bool[] usedSlots = new bool[CustomLevelManager.Instance.possibleIconLocation.Length];

        foreach (string file in files)
        {
            string json = File.ReadAllText(file);
            LevelData data = JsonUtility.FromJson<LevelData>(json);

            if (data.icons.Count == 0)
                continue;

            LevelIconData icon = data.icons[0];

            int index = icon.placeholderIndex;

            // If slot is taken, find next free one
            if (index < 0 || index >= usedSlots.Length || usedSlots[index])
            {
                index = FindNextFreeSlot(usedSlots);

                // Update icon data
                icon.placeholderIndex = index;

                // Save updated placeholderIndex back to file
                data.icons[0] = icon;
                string updatedJson = JsonUtility.ToJson(data, true);
                File.WriteAllText(file, updatedJson);

                Debug.Log("Updated placeholderIndex for " + icon.iconName + " to slot " + index);
            }

            // Mark slot as used
            usedSlots[index] = true;

            // Add icon to UI list
            CL_ButtonManager.Instance.icons.Add(icon);
        }

        CL_ButtonManager.Instance.RebuildButtonsFromData();
        Debug.Log("Loaded all level buttons.");
    }

    public void LoadLevelObjects(LevelIconData icon)
    {
        birdOrder = 1;

        //string folder = Path.Combine(Application.persistentDataPath, saveFileFolderName);
        string folder = Path.Combine(Directory.GetParent(Application.dataPath).FullName, saveFileFolderName);

        string filePath = Path.Combine(folder, $"{icon.levelID}.json");

        if (!File.Exists(filePath))
        {
            Debug.LogWarning("Level file not found: " + filePath);
            return;
        }

        string json = File.ReadAllText(filePath);
        LevelData data = JsonUtility.FromJson<LevelData>(json);

        foreach (var obj in placedObjects)
            Destroy(obj);
        placedObjects.Clear();

        foreach (var objData in data.objects)
        {
            GameObject prefab = FindPrefabByName(objData.prefabName);
            if (prefab != null)
            {
                GameObject obj = Instantiate(prefab, objData.position, objData.rotation);

                Bird bird = obj.GetComponent<Bird>();
                if (bird != null)
                {
                    bird.birdOrder = objData.birdOrder;
                    birdOrder++;
                }

                placedObjects.Add(obj);
            }
        }

        Debug.Log("Loaded level objects.");
    }

    public void DeleteLevel(LevelIconData icon)
    {
        //string folder = Path.Combine(Application.persistentDataPath, saveFileFolderName);
        string folder = Path.Combine(Directory.GetParent(Application.dataPath).FullName, saveFileFolderName);

        string filePath = Path.Combine(folder, $"{icon.levelID}.json");
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log("Deleted level file: " + filePath);
        }
        else
        {
            Debug.LogWarning("Level file not found for deletion: " + filePath);
        }
    }

    public void SaveLevelLocally()
    {
        LevelData data = new LevelData();

        foreach (var obj in placedObjects)
        {
            LevelObjectData objData = new LevelObjectData();
            objData.prefabName = obj.name.Replace("(Clone)", "").Trim();
            objData.position = obj.transform.position;
            objData.rotation = obj.transform.rotation;

            Bird bird = obj.GetComponent<Bird>();
            if (bird != null)
            {
                objData.birdOrder = bird.birdOrder;
            }

            data.objects.Add(objData);
        }
        // Save as JSON string in memory only
        savedLevelJson = JsonUtility.ToJson(data, true);

        Debug.Log("Level saved locally (not written to file).");
    }

    // Load a level from memory instead of file
    public void LoadLevelLocally()
    {
        if (string.IsNullOrEmpty(savedLevelJson))
        {
            Debug.LogWarning("No saved level found in memory.");
            return;
        }

        GameObject[] gos = GameObject.FindGameObjectsWithTag("Bird");
        foreach (GameObject go in gos)
            Destroy(go);

        // Clear existing objects
        foreach (var obj in placedObjects)
        {
            Destroy(obj);
        }
        placedObjects.Clear();

        // Deserialize from memory
        LevelData data = JsonUtility.FromJson<LevelData>(savedLevelJson);

        foreach (var objData in data.objects)
        {
            GameObject prefab = FindPrefabByName(objData.prefabName);
            if (prefab != null)
            {
                GameObject obj = Instantiate(prefab, objData.position, objData.rotation);

                Bird bird = obj.GetComponent<Bird>();
                if (bird != null)
                {
                    bird.birdOrder = objData.birdOrder;
                    //birdOrder++;
                }

                placedObjects.Add(obj);
            }
        }

        Debug.Log("Level loaded from local memory.");
    }

    public void OpenLevel(LevelIconData iconData)
    {
        iconDataLM = iconData;
        SceneManager.LoadScene("CustomLevelTemplate"); // Ensure you're on the correct scene
    }
    public void ExitToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private int FindNextFreeSlot(bool[] usedSlots)
    {
        for (int i = 0; i < usedSlots.Length; i++)
        {
            if (!usedSlots[i])
                return i;
        }

        Debug.LogWarning("No free icon slots available!");
        return 0; // fallback
    }

    public void PlayGame()
    {
        if (GameObject.FindGameObjectsWithTag("Pig").Length == 0 ||
    GameObject.FindGameObjectsWithTag("Bird").Length == 0)
        {
            Debug.LogWarning("Cannot start game: missing Pig or Bird in scene.");
            return;
        }

        SaveLevelLocally();
        gameManager.GameManager_Activate();
        slingShot.SlingShot_Activate();
        playGame.SetActive(false);
        buildGame.SetActive(true);
        foreach (BrickManager brickScript in brickManagerScripts)
        {
            //brickScript.SetLocation();
            brickScript.buildMode = !brickScript.buildMode;
        }
        foreach (PigManager pigScript in pigManagerScripts)
        {
            //pigScript.SetLocation();
            //pigScript.buildMode = !pigScript.buildMode;
            pigScript.buildMode = false;
            Debug.Log("Pig.Buildmode switch on: " + pigScript.gameObject.name);
        }
        foreach (Bird birdScript in birdScript)
        {
            if (birdScript != null)
            {
                birdScript.Bird_Activate();
            }
        }
    }

    public void BuildGame()
    {
        /*
        foreach (BrickManager brickScript in brickManagerScripts)
        {
            brickScript.ResetLocation();
            brickScript.buildMode = !brickScript.buildMode;
        }
        foreach (PigManager pigScript in pigManagerScripts)
        {
            pigScript.ResetLocation();
            pigScript.buildMode = !pigScript.buildMode;
        }
        */
        DOTween.KillAll(true); // <- important: TRUE forces completion kill
        playGame.SetActive(true);
        buildGame.SetActive(false);
        LoadLevelLocally();
        gameManager.GameManager_Reset();
        slingShot.SlingShot_Reset();
        GetPlacedObjScripts();
        pathPoints.Clear();
        score.scoreReset();
    }

    // Helper: find prefab by name
    private GameObject FindPrefabByName(string name)
    {
        foreach (var prefab in placeablePrefabs)
        {
            if (prefab.name == name)
                return prefab;
        }
        Debug.LogWarning("Prefab not found for: " + name);
        return null;
    }
}