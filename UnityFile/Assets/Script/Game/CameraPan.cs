using Photon.Pun;
using UnityEngine;

public class CameraPan : MonoBehaviourPun
{
    private Vector3 previousMousePosition;
    private bool isPanning;

    [Header("Pan Settings")]
    public float panSpeed = 0.5f; // Speed of the pan
    public float smoothSpeed = 0.1f; // Speed of smooth movement (higher value = faster smoothing)

    [Header("Zoom Settings")]
    public float zoomSpeed = 2f; // Speed of zooming
    public float minSize = 5f; // Minimum orthographic size (zoomed in)
    public float maxSize = 20f; // Maximum orthographic size (zoomed out)

    private Vector3 targetPosition; // Target position the camera is moving towards
    private Camera cameraComponent; // Reference to the Camera component

    private void Start()
    {
        // Get the camera component on the GameObject this script is attached to
        cameraComponent = GetComponent<Camera>();
    }

    private void Update()
    {

        // Check if the middle mouse button is held down
        if (Input.GetMouseButtonDown(2)) // 2 is for middle mouse button
        {
            isPanning = true;
            previousMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(2))
        {
            isPanning = false;
        }

        // If panning is active, calculate the movement
        if (isPanning)
        {
            PanCamera();
        }

        // Apply smooth movement to the camera
        if (targetPosition != Vector3.zero)
        {
            SmoothMoveCamera();
        }

        // Zoom in and out based on the mouse scroll wheel input
        ZoomCamera();
    }

    private void PanCamera()
    {
        // Get the delta position of the mouse
        Vector3 deltaMousePosition = Input.mousePosition - previousMousePosition;
        previousMousePosition = Input.mousePosition;

        // Convert deltaMousePosition to world space
        Vector3 move = new Vector3(deltaMousePosition.x, deltaMousePosition.y, 0f);
        move = move * panSpeed * Time.deltaTime;

        // Set the target position for the camera
        targetPosition = transform.position - move;  // Subtract to move the camera in the opposite direction
    }

    private void SmoothMoveCamera()
    {
        // Smoothly move the camera towards the target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);
    }

    private void ZoomCamera()
    {
        // Get the scroll wheel input (positive for zooming in, negative for zooming out)
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        // Adjust the camera's orthographic size based on the scroll input
        if (scrollInput != 0f)
        {
            // Zoom by adjusting the orthographic size
            cameraComponent.orthographicSize -= scrollInput * zoomSpeed;

            // Clamp the orthographic size to prevent it from going too far
            cameraComponent.orthographicSize = Mathf.Clamp(cameraComponent.orthographicSize, minSize, maxSize);
        }
    }
}
