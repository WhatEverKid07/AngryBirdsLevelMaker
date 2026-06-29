using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Panning Settings")]
    [SerializeField] private float panSpeed = 5f;

    [Header("Bounds")]
    [SerializeField] private float minX;
    [SerializeField] private float maxX;
    [SerializeField] private float minY;
    [SerializeField] private float maxY;

    
    [Header("Zoom Settings")]
    private Camera cam;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float smoothTime = 0.25f;
    [SerializeField] private float minZoom;
    [SerializeField] private float maxZoom;
    [SerializeField] private float zoom;
    

    private Vector3 dragOrigin;

    private void Start()
    {
        cam = Camera.main;
        zoom = cam.orthographicSize;
        minZoom = cam.orthographicSize;
        maxZoom = cam.orthographicSize / 10f;
        //Zoom = Cam.orthographicSize;
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(2))
        {
            dragOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        if (Input.GetMouseButton(2))
        {
            Vector3 difference = dragOrigin - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position += difference;
            transform.position = new Vector3
            (
                Mathf.Clamp(transform.position.x, minX, maxX),
                Mathf.Clamp(transform.position.y, minY, maxY),
                transform.position.z
            );
        }
        
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        zoom -= scroll * zoomSpeed;
        zoom = Mathf.Clamp(zoom, maxZoom, minZoom);
        cam.orthographicSize = zoom;
        //cam.orthographicSize = Mathf.SmoothDamp(zoom, ref zoomSpeed);
    }
}