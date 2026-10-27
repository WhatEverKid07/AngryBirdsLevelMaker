using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using System.Linq;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    // vars

    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private SlingShot slingshot;

    //[HideInInspector]

    public static GameState CurrentGameState = GameState.Start;

    public static List<GameObject> Bricks;
    public static List<GameObject> Birds;
    public static List<GameObject> BirdsCopy;
    public static List<GameObject> Pigs;
    public static List<GameObject> AliveBirds;

    public static int currentBirdIndex;

    //[HideInInspector]
    public List<Vector2> vectors;
    //[HideInInspector]
    public List<Vector2> vectorsCopy;

    public static int birdsNumber;

    [SerializeField] private bool gameManager_Is_Active = false;

    // voids 
    private void Start()
    {
        if (!gameManager_Is_Active)
            return;
        // initialize slingshot and game state

        CurrentGameState = GameState.Start;
        slingshot.enabled = true;

        slingshot.BirdThrown -= Slingshot_BirdThrown; slingshot.BirdThrown += Slingshot_BirdThrown;
        birdList();

        foreach (var obj in Birds)
        {
            vectors.Add(obj.transform.position);
        }

        vectorsCopy = vectors;
    }

    private void Awake()
    {
        AliveBirds = null;
        vectors = null;
        vectorsCopy = null;
        Bricks = null;
        Birds = null;
        BirdsCopy = null;
        Pigs = null;

        if (!gameManager_Is_Active)
            return;
        
        currentBirdIndex = 0;

        // define lists

        AliveBirds = new List<GameObject>();
        Bricks = new List<GameObject>(GameObject.FindGameObjectsWithTag("Brick"));
        Pigs = new List<GameObject>(GameObject.FindGameObjectsWithTag("Pig"));

        Birds = new List<GameObject>();
        BirdsCopy = new List<GameObject>();
        vectors = new List<Vector2>();
        vectorsCopy = new List<Vector2>();


        // change physics back to normal

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.01f;
    }

    public void GameManager_Activate()
    {
        gameManager_Is_Active = true;
        currentBirdIndex = 0;

        // define lists

        AliveBirds = new List<GameObject>();
        Bricks = new List<GameObject>(GameObject.FindGameObjectsWithTag("Brick"));
        Pigs = new List<GameObject>(GameObject.FindGameObjectsWithTag("Pig"));

        Birds = new List<GameObject>();
        BirdsCopy = new List<GameObject>();
        vectors = new List<Vector2>();
        vectorsCopy = new List<Vector2>();


        // change physics back to normal

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.01f;

        // From Start
        // initialize slingshot and game state

        CurrentGameState = GameState.Start;
        slingshot.enabled = true;

        slingshot.BirdThrown -= Slingshot_BirdThrown; slingshot.BirdThrown += Slingshot_BirdThrown;
        birdList();

        foreach (var obj in Birds)
        {
            vectors.Add(obj.transform.position);
        }

        vectorsCopy = vectors;
        Debug.Log("GameManager Activate");
    }

    public void GameManager_Reset()
    {
        DOTween.KillAll(true);
        StopAllCoroutines();
        gameManager_Is_Active = false;
        // Reset Game State
        CurrentGameState = GameState.Start;

        // Reset lists (fresh like Awake)
        Birds = new List<GameObject>();
        BirdsCopy = new List<GameObject>();
        AliveBirds = new List<GameObject>();
        Bricks = new List<GameObject>();
        Pigs = new List<GameObject>();
        vectors = new List<Vector2>();
        vectorsCopy = new List<Vector2>();

        birdsNumber = 0;
        currentBirdIndex = 0;

        // Reset slingshot
        slingshot.enabled = true;
        slingshot.slingshotState = SlingshotState.Idle;
        slingshot.BirdThrown -= Slingshot_BirdThrown; // avoid double subscription
        slingshot.BirdThrown += Slingshot_BirdThrown;

        // Reset camera
        cameraFollow.IsFollowing = false;
        cameraFollow.BirdToFollow = null;

        // Reset Time/Physics
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.01f;

        Debug.Log("GameManager Reset");
    }

    private void Update()
    {
        Debug.Log(currentBirdIndex);


        if (!gameManager_Is_Active)
            return;

        if (AllPigsDestroyed())
        {
            CurrentGameState = GameState.Won;
        }

        switch (CurrentGameState)
        {
            case GameState.Won:
                StartCoroutine(cameMoveDelayCour());
                break;
            case GameState.Lost:
                StartCoroutine(cameMoveDelayCour());
                break;
            case GameState.Start:
                AnimateBirdToSlingshot();
                break;
            case GameState.BirdMovingToSlingshot:
                break;
            case GameState.Playing:
                if (slingshot.slingshotState == SlingshotState.BirdFlying && (BricksBirdsPigsStoppedMoving() || Time.time - slingshot.TimeSinceThrown > 5f))
                {
                    StartCoroutine(delayCour());
                }
                break;
            default:
                break;
        }
    }

    private void birdList()
    {
        List<GameObject> objectsWithTag = new List<GameObject>(GameObject.FindGameObjectsWithTag("Bird"));

        List<int> birdList = new List<int>();
        List<int> birdListCopy = new List<int>();
        List<int> originalIndex = new List<int>();

        foreach (var obj in objectsWithTag)
        {
            birdList.Add(obj.GetComponent<Bird>().birdOrder);
            birdListCopy.Add(obj.GetComponent<Bird>().birdOrder);
        }

        birdList.Sort();

        int index;

        foreach (var obj in birdList)
        {
            index = 0;
            foreach (var obj2 in birdListCopy)
            {
                if (obj == obj2)
                {
                    originalIndex.Add(index);
                    break;
                }
                index++;
            }
        }
        foreach (var obj in originalIndex)
        {
            Birds.Add(objectsWithTag[obj]);
            BirdsCopy.Add(objectsWithTag[obj]);
        }
        birdsNumber = Birds.Count;

    }
    
    private void AnimateCameraToStartPosition()
    {
        float duration = Vector2.Distance(Camera.main.transform.position, cameraFollow.StartingPosition) / 2.5f;
        if (duration == 0.0f) duration = 0.1f;
        //animate the camera to start
        Camera.main.transform.DOMove(cameraFollow.StartingPosition, duration);

        cameraFollow.IsFollowing = false;

        if (AllPigsDestroyed())
        {
            CurrentGameState = GameState.Won;
        }
        else if (currentBirdIndex == Birds.Count - 1 && CurrentGameState != GameState.Won && birdsNumber == 0)
        {
            CurrentGameState = GameState.Lost;
        }
        else
        {
            slingshot.slingshotState = SlingshotState.Idle;
            currentBirdIndex++;

            if (currentBirdIndex >= Birds.Count)
            {
                Debug.Log("No more birds");
                return;
            }

            if (GetCurrentBirdObject() != null)
            {
                AnimateBirdToSlingshot();
            }
        }
    }

    private void AnimateBirdToSlingshot()
    {
        if (currentBirdIndex < 0 || currentBirdIndex >= Birds.Count)
        {
            Debug.LogWarning("AnimateBirdToSlingshot blocked: invalid index");
            return;
        }

        GameObject bird = Birds[currentBirdIndex];
        if (bird == null)
        {
            Debug.LogWarning("AnimateBirdToSlingshot blocked: bird is null");
            return;
        }

        StartCoroutine(moveBirdToNextPost());
        CurrentGameState = GameState.BirdMovingToSlingshot;

        bird.transform
            .DOJump(
                slingshot.BirdWaitPosition.transform.position,
                2f,
                1,
                Vector2.Distance(bird.transform.position, slingshot.BirdWaitPosition.transform.position) / 2f
            )
            .SetLink(bird) // 🔥 IMPORTANT FIX
            .OnComplete(() => completedAnimate());
    }

    //private void AnimateBirdToSlingshot()
    //{
    //    StartCoroutine(moveBirdToNextPost());
    //    CurrentGameState = GameState.BirdMovingToSlingshot;
    //    //AudioPlayer.audio.PlayOneShot(Birds[currentBirdIndex].GetComponent<Bird>().selectSound[0]);
    //    Birds[currentBirdIndex].transform.DOJump(slingshot.BirdWaitPosition.transform.position, 2f, 1, Vector2.Distance(Birds[currentBirdIndex].transform.position, slingshot.BirdWaitPosition.transform.position) / 2f).OnComplete(() => completedAnimate());
    //}
    /*private void AnimateBirdToSlingshot()
    {
        if (currentBirdIndex < 0 || currentBirdIndex >= Birds.Count)
        {
            //Debug.LogWarning($"Invalid currentBirdIndex={currentBirdIndex}, Birds.Count={Birds.Count}");
            return; // nothing to animate
        }

        StartCoroutine(moveBirdToNextPost());
        CurrentGameState = GameState.BirdMovingToSlingshot;

        var bird = GetCurrentBirdObject();
        if (bird == null) return;

        bird.transform.DOJump(
            slingshot.BirdWaitPosition.transform.position,
            2f,
            1,
            Vector2.Distance(bird.transform.position, slingshot.BirdWaitPosition.transform.position) / 2f
        ).OnComplete(() => completedAnimate());
    }*/


    private void completedAnimate()
    {
        GameObject bird = GetCurrentBirdObject();

        if (bird == null)
        {
            Debug.LogWarning("completedAnimate: no valid bird");
            return;
        }

        CurrentGameState = GameState.Playing;
        slingshot.enabled = true;
        slingshot.BirdToThrow = bird;

        var audio = bird.GetComponent<AudioSource>();
        var birdComp = bird.GetComponent<Bird>();

        if (audio != null && birdComp != null && birdComp.selectSound.Length > 0)
        {
            //audio.PlayOneShot(birdComp.selectSound[0]);
        }
    }
    //private void completedAnimate()
    //{
    //    if (Birds == null || Birds.Count == 0)
    //    {
    //        Debug.LogWarning("completedAnimate called with no birds");
    //        return;
    //    }

    //    if (currentBirdIndex < 0 || currentBirdIndex >= Birds.Count)
    //    {
    //        Debug.LogWarning("Invalid bird index");
    //        return;
    //    }
    //    CurrentGameState = GameState.Playing;
    //    slingshot.enabled = true;
    //    slingshot.BirdToThrow = Birds[currentBirdIndex];
    //    Birds[currentBirdIndex].GetComponent<AudioSource>().PlayOneShot(Birds[currentBirdIndex].GetComponent<Bird>().selectSound[0]);
    //}
    /*private void completedAnimate()
    {
        var bird = GetCurrentBirdObject();
        if (bird == null)
        {
            Debug.LogWarning($"completedAnimate skipped: invalid index {currentBirdIndex}, Birds.Count={Birds.Count}");
            return;
        }

        CurrentGameState = GameState.Playing;
        slingshot.enabled = true;
        slingshot.BirdToThrow = bird;
        bird.GetComponent<AudioSource>().PlayOneShot(bird.GetComponent<Bird>().selectSound[0]);
    }*/

    /*private void Slingshot_BirdThrown(object sender, System.EventArgs e)
    {
        cameraFollow.BirdToFollow = Birds[currentBirdIndex].transform;
        cameraFollow.IsFollowing = true;
    }*/
    private void Slingshot_BirdThrown(object sender, System.EventArgs e)
    {
        var bird = GetCurrentBirdObject();
        if (bird == null)
        {
            Debug.LogWarning("Slingshot_BirdThrown skipped — no valid bird");
            return;
        }

        cameraFollow.BirdToFollow = bird.transform;
        cameraFollow.IsFollowing = true;
    }

    // var voids
    private bool AllPigsDestroyed()
    {
        return Pigs.All(x => x == null);
    }

    public static bool BricksBirdsPigsStoppedMoving()
    {
        foreach (var item in Bricks.Union(Birds).Union(Pigs))
        {
            if (item != null && item.GetComponent<Rigidbody2D>().linearVelocity.sqrMagnitude > Constants.MinVelocity)
            {
                return false;
            }
        }

        return true;
    }

    public static void AutoResize(int screenWidth, int screenHeight)
    {
        Vector2 resizeRatio = new Vector2((float)Screen.width / screenWidth, (float)Screen.height / screenHeight);
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(resizeRatio.x, resizeRatio.y, 1.0f));
    }


    // enums 


    private IEnumerator cameMoveDelayCour()
    {
        yield return new WaitForSeconds(1.5f);
        AnimateCameraToStartPosition();
    }


    private IEnumerator delayCour()
    {
        slingshot.enabled = true;
        if (currentBirdIndex == Birds.Count - 1 && CurrentGameState != GameState.Won && birdsNumber == 0)
        {
            CurrentGameState = GameState.Lost;
        }
        else
        {
            CurrentGameState = GameState.BirdMovingToSlingshot;
        }
        yield return new WaitForSeconds(1f);
        AnimateCameraToStartPosition();
    }



    public IEnumerator moveBirdToNextPost()
    {
        if (BirdsCopy.Count > 0)
        {
            BirdsCopy.RemoveAt(0);
        }
        int birdCount = BirdsCopy.Count();
        int birdIndex = 0;
        bool isRunning = false;

        yield return new WaitForSeconds(0.4f);

        void isRunningComplete()
        {
            isRunning = false;
        }

        while (birdCount > birdIndex)
        {
            yield return new WaitForSeconds(0.1f);

            if (!isRunning)
            {
                isRunning = true;
                BirdsCopy[birdIndex].transform.DOJump(vectorsCopy[birdIndex], 2f, 1, Vector2.Distance(BirdsCopy[birdIndex].transform.position, vectorsCopy[birdIndex]) / 2f).OnComplete(() => isRunningComplete());
                birdIndex++;
            }
        }
    }

    public static GameObject GetCurrentBirdObject()
    {
        if (currentBirdIndex >= 0 && currentBirdIndex < Birds.Count)
            return Birds[currentBirdIndex];
        return null;
    }
}
