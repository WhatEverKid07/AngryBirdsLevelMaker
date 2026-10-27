using UnityEngine;
using System.Collections;
using Assets.Scripts;
using System;
using DG.Tweening;

public class SlingShot : MonoBehaviour
{
    public Vector3 SlingshotMiddleVector;

    [HideInInspector]
    public SlingshotState slingshotState;
    [SerializeField] private Transform LeftSlingshotOrigin, RightSlingshotOrigin;
    [SerializeField] private LineRenderer SlingshotLineRenderer1;
    [SerializeField] private LineRenderer SlingshotLineRenderer2;
    [SerializeField] private LineRenderer TrajectoryLineRenderer;

    [HideInInspector]
    public GameObject BirdToThrow;
    [SerializeField] public GameObject slingShotEnd;
    public Transform BirdWaitPosition;
    [SerializeField] private float ThrowSpeed;

    [HideInInspector]
    public float TimeSinceThrown;
    private AudioSource audio;
    private bool lastPullCheck = false;
    [SerializeField] private AudioClip[] militaryClips;
    [SerializeField] private AudioClip[] shootClips;

    [SerializeField] private bool slingShot_Is_Active = false;
    private void playSound(AudioClip clip)
    {
        audio.PlayOneShot(clip);
    }
    private void Awake()
    {
        if (!slingShot_Is_Active)
            return;

        slingShotEnd.SetActive(true);
        //SlingshotLineRenderer1.sortingLayerName = "Foreground";
        //SlingshotLineRenderer2.sortingLayerName = "Foreground";
        //TrajectoryLineRenderer.sortingLayerName = "Foreground";

        slingshotState = SlingshotState.Idle;
        SlingshotLineRenderer1.SetPosition(0, LeftSlingshotOrigin.position);
        SlingshotLineRenderer2.SetPosition(0, RightSlingshotOrigin.position);


        SlingshotMiddleVector = new Vector3((LeftSlingshotOrigin.position.x + RightSlingshotOrigin.position.x) / 2,
            (LeftSlingshotOrigin.position.y + RightSlingshotOrigin.position.y) / 2, 0);

        audio = gameObject.GetComponent<AudioSource>();

        audio.PlayOneShot(militaryClips[UnityEngine.Random.Range(0, 4)]);

        //DisplaySlingshotLineRenderers();

        SetSlingshotLineRenderersActive(true);

        SlingshotLineRenderer1.SetPosition(1, BirdWaitPosition.transform.position);
        SlingshotLineRenderer2.SetPosition(1, BirdWaitPosition.transform.position);
    }

    public void SlingShot_Activate()
    {
        slingShotEnd.SetActive(true);
        slingShot_Is_Active = true;
        slingshotState = SlingshotState.Idle;
        SlingshotLineRenderer1.SetPosition(0, LeftSlingshotOrigin.position);
        SlingshotLineRenderer2.SetPosition(0, RightSlingshotOrigin.position);
        SlingshotMiddleVector = new Vector3((LeftSlingshotOrigin.position.x + RightSlingshotOrigin.position.x) / 2,
            (LeftSlingshotOrigin.position.y + RightSlingshotOrigin.position.y) / 2, 0);
        audio = gameObject.GetComponent<AudioSource>();
        audio.PlayOneShot(militaryClips[UnityEngine.Random.Range(0, 4)]);
        SetSlingshotLineRenderersActive(true);
        SlingshotLineRenderer1.SetPosition(1, BirdWaitPosition.transform.position);
        SlingshotLineRenderer2.SetPosition(1, BirdWaitPosition.transform.position);
        Debug.Log("Slingshot_Activate");
    }
    public void SlingShot_Reset()
    {
        slingShot_Is_Active = false;
        slingShotEnd.SetActive(false);
        slingshotState = SlingshotState.Idle;
        SlingshotMiddleVector = new Vector3(0, 0, 0);
        SetSlingshotLineRenderersActive(false);
        SlingshotLineRenderer1.SetPosition(1, BirdWaitPosition.transform.position);
        SlingshotLineRenderer2.SetPosition(1, BirdWaitPosition.transform.position);
        Debug.Log("Slingshot_Reset");
    }

    private void Update()
    {
        if (!slingShot_Is_Active)
            return;

        if (BirdToThrow != null)
        {
            
            if (BirdWaitPosition.transform.position.x - BirdToThrow.transform.position.x <= 0.3f && BirdWaitPosition.transform.position.x - BirdToThrow.transform.position.x >= -0.3f && !(BirdToThrow.GetComponent<Bird>().Collided))
            {
                float maxPosY = Mathf.Clamp(BirdToThrow.transform.position.y, BirdWaitPosition.transform.position.y - 0.5f, BirdWaitPosition.transform.position.y + 100);
                BirdToThrow.transform.position = new Vector3(BirdToThrow.transform.position.x, maxPosY, BirdToThrow.transform.position.z);
            }

            //slingShotEnd.transform.position = new Vector3(BirdToThrow.transform.position.x, BirdToThrow.transform.position.y, BirdToThrow.transform.position.z);

        }
        if (BirdToThrow != null && slingshotState == SlingshotState.UserPulling)
        {
            slingShotEnd.SetActive(true);
            Vector2 direction = SlingshotMiddleVector - slingShotEnd.transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            slingShotEnd.transform.rotation = Quaternion.Slerp(slingShotEnd.transform.rotation, rotation,1f);

            Vector3 positionToCollider = BirdToThrow.GetComponent<Bird>().slingShotCol.transform.position - SlingshotMiddleVector;
            Vector3 otherSide = BirdToThrow.GetComponent<Bird>().slingShotCol.transform.position + positionToCollider;
            Vector3 farthersPoint = BirdToThrow.GetComponent<Bird>().slingShotCol.ClosestPoint(otherSide);

            slingShotEnd.transform.position = farthersPoint;
        }
        else
        {
            slingShotEnd.SetActive(false);
        }

        if (lastPullCheck == false && slingshotState == SlingshotState.UserPulling)
        {
            audio.Play();
        }
        if (slingshotState == SlingshotState.UserPulling)
        {
            lastPullCheck = true;
        }
        else
        {
            lastPullCheck = false;
        }

        switch (slingshotState)
        {
            case SlingshotState.Idle:

                SlingshotLineRenderer1.material.renderQueue = 2000;

                InitializeBird();

                DisplaySlingshotLineRenderers();

                if (Input.GetMouseButtonDown(0))
                {

                    Vector3 location = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    if (BirdToThrow != null)
                    {
                        if (BirdToThrow.GetComponent<CircleCollider2D>() == Physics2D.OverlapPoint(location) || BirdToThrow.GetComponent<Bird>().slingShotCol == Physics2D.OverlapPoint(location) && GameManager.CurrentGameState != GameState.Won && GameManager.CurrentGameState != GameState.Lost)
                        {
                            slingshotState = SlingshotState.UserPulling;
                        }
                    }
                }
                break;
            case SlingshotState.UserPulling:
                DisplaySlingshotLineRenderers();

                SlingshotLineRenderer1.material.renderQueue = 3000;

                if (Input.GetMouseButton(0))
                {
                    Vector3 location = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    location.z = 0;
                    if (Vector3.Distance(location, SlingshotMiddleVector) > 1.5f)
                    {
                        var maxPosition = (location - SlingshotMiddleVector).normalized * 1.5f + SlingshotMiddleVector;

                        if (BirdWaitPosition.transform.position.x - BirdToThrow.transform.position.x <= 0.3f && BirdWaitPosition.transform.position.x - BirdToThrow.transform.position.x >= -0.3f)
                        {
                            float maxPosY = Mathf.Clamp(maxPosition.y, BirdWaitPosition.transform.position.y - 0.5f, BirdWaitPosition.transform.position.y + 100);
                            maxPosition.y = maxPosY;
                        }
                        BirdToThrow.transform.position = maxPosition;
                    }
                
                    else
                    {
                        if(BirdWaitPosition.transform.position.x - BirdToThrow.transform.position.x <= 0.3f && BirdWaitPosition.transform.position.x - BirdToThrow.transform.position.x >= -0.3f)
                        {
                            float maxPosY = Mathf.Clamp(location.y, BirdWaitPosition.transform.position.y - 0.5f, BirdWaitPosition.transform.position.y + 100);
                            location.y = maxPosY;
                            BirdToThrow.transform.position = location;
                        }
                        else
                        {
                            BirdToThrow.transform.position = location;
                        }
                    }
                    //float distance = Vector3.Distance(SlingshotMiddleVector, BirdToThrow.transform.position);
                    //DisplayTrajectoryLineRenderer2(distance);
                }
                else
                {
                    SlingshotLineRenderer1.SetPosition(1, BirdWaitPosition.transform.position);
                    SlingshotLineRenderer2.SetPosition(1, BirdWaitPosition.transform.position);
                    //SetTrajectoryLineRenderesActive(false);

                    TimeSinceThrown = Time.time;

                    float distance = Vector3.Distance(SlingshotMiddleVector, BirdToThrow.transform.position);

                    if (distance > 0.75f)
                    {
                        audio.PlayOneShot(shootClips[UnityEngine.Random.Range(0, 3)], 0.5f);

                        slingshotState = SlingshotState.BirdFlying;
                        ThrowBird(distance);
                    }
                    else
                    {
                        BirdToThrow.transform.DOMove(BirdWaitPosition.transform.position, distance / 10);
                        InitializeBird();

                    }
                }
                break;
            case SlingshotState.BirdFlying:
                break;
            default:
                break;
        }

    }

    private void ThrowBird(float distance)
    {

        Vector3 velocity = SlingshotMiddleVector - BirdToThrow.transform.position;
        BirdToThrow.GetComponent<Bird>().OnThrow();

        BirdToThrow.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(velocity.x, velocity.y) * ThrowSpeed * distance;

        GameManager.birdsNumber = GameManager.birdsNumber - 1;

        if (BirdThrown != null)
            BirdThrown(this, EventArgs.Empty);
    }


    private void InitializeBird()
    {
        if(BirdToThrow != null)
        {
            if(!(BirdToThrow.GetComponent<Bird>().Collided))
            {
                BirdToThrow.transform.position = BirdWaitPosition.position;
                slingshotState = SlingshotState.Idle;
                SetSlingshotLineRenderersActive(true);
            }
        }
    }

    private void DisplaySlingshotLineRenderers()
    {
        if(BirdToThrow != null)
        {
            if(!(BirdToThrow.GetComponent<Bird>().Collided) && slingshotState == SlingshotState.UserPulling)
            {
                SlingshotLineRenderer1.SetPosition(1, slingShotEnd.transform.position);
                SlingshotLineRenderer2.SetPosition(1, slingShotEnd.transform.position);
            }
            else
            {
                SlingshotLineRenderer1.SetPosition(1, BirdWaitPosition.transform.position);
                SlingshotLineRenderer2.SetPosition(1, BirdWaitPosition.transform.position);
            }
        }
        else
        {
            SlingshotLineRenderer1.SetPosition(1, BirdWaitPosition.transform.position);
            SlingshotLineRenderer2.SetPosition(1, BirdWaitPosition.transform.position);
        }
    }

    private void SetSlingshotLineRenderersActive(bool active)
    {
        SlingshotLineRenderer1.enabled = active;
        SlingshotLineRenderer2.enabled = active;
    }

    private void SetTrajectoryLineRenderesActive(bool active)
    {
        TrajectoryLineRenderer.enabled = active;
    }

    private void DisplayTrajectoryLineRenderer2(float distance)
    {
        SetTrajectoryLineRenderesActive(true);
        Vector3 v2 = SlingshotMiddleVector - BirdToThrow.transform.position;
        int segmentCount = 15;
        float segmentScale = 2;
        Vector2[] segments = new Vector2[segmentCount];

        segments[0] = BirdToThrow.transform.position;

        Vector2 segVelocity = new Vector2(v2.x, v2.y) * ThrowSpeed * distance;

        float angle = Vector2.Angle(segVelocity, new Vector2(1, 0));
        float time = segmentScale / segVelocity.magnitude;
        for (int i = 1; i < segmentCount; i++)
        {
            float time2 = i * Time.fixedDeltaTime * 5;
            segments[i] = segments[0] + segVelocity * time2 + 0.5f * Physics2D.gravity * Mathf.Pow(time2, 2);
        }

        TrajectoryLineRenderer.SetVertexCount(segmentCount);
        for (int i = 0; i < segmentCount; i++)
            TrajectoryLineRenderer.SetPosition(i, segments[i]);
    }



    public event EventHandler BirdThrown;
}
