using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ObjectMovRot : MonoBehaviour
{
    private bool dragging = false;
    private Vector3 offset;
    [HideInInspector] internal bool isPlacing = false;
    [SerializeField] internal bool canMove = true;
    
    [Header("Rotation Settings")]
    [SerializeField] private bool snapRotation = true;
    [SerializeField] private float rotationStep = 45f;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float snapCooldown = 0.2f;

    private float snapTimer = 0f;

    void Update()
    {
        //if (isPlacing)
        //{
        //    Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //    mouseWorld.z = transform.position.z;
        //    transform.position = mouseWorld + offset;
        //}
        //if (snapTimer > 0f)
        //    snapTimer -= Time.deltaTime;


        if (isPlacing && canMove)
        {
            dragging = true;
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = transform.position.z;
            transform.position = mouseWorld + offset;

            if (Input.GetMouseButtonUp(0))
            {
                isPlacing = false;
                dragging = false;
            }
        }

        if (snapTimer > 0f)
            snapTimer -= Time.deltaTime;
    }

    /*
    private void OnMouseDown()
    {
        if (Input.GetMouseButton(1))
        {
            RotateObject();
            return;
        }
        if (Input.GetMouseButton(0))
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = transform.position.z;
            offset = transform.position - mouseWorld;
            dragging = true;
        }
        isPlacing = !isPlacing;
    }
    
    private void OnMouseUp()
    { dragging = false; }
    */

    private void OnMouseDown()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = transform.position.z;

        offset = transform.position - mouseWorld;
        isPlacing = true;
        dragging = true;
    }
    private void OnMouseUp()
    {
        isPlacing = false;
        dragging = false;
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButton(1) && !dragging)
        {
            LevelManager.Instance.RemoveObject(gameObject);
            //RotateObject();
        }
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            RotateObject(scroll);
        }
    }

    private void RotateObject(float scroll)
    {
        /*
        if (snapRotation)
        {
            if (snapTimer <= 0f)
            {
                transform.rotation *= Quaternion.Euler(0, 0, rotationStep);
                snapTimer = snapCooldown;
            }
        } 
        else { transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime, Space.World); }
        */
        if (snapRotation)
        {
            // Scroll up = +step, scroll down = -step
            transform.rotation *= Quaternion.Euler(0, 0, rotationStep * Mathf.Sign(scroll));
        }
        else
        {
            // Smooth rotation
            transform.Rotate(Vector3.forward, scroll * rotationSpeed, Space.World);
        }
    }
}