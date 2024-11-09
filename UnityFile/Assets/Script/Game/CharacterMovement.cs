using Photon.Pun;
using UnityEngine;

public class CharacterMovement : MonoBehaviourPun
{
    private Vector3 mouseOffset;
    private bool isDragging = false;
    private Camera mainCamera;

    [Header("Movement Settings")]
    public float dragSpeed = 2f;

    private void Start()
    {
        // Get reference to the main camera
        mainCamera = Camera.main;
    }

    private void Update()
    {
        // Allow drag only for local player or Host (DM)
        if (!(photonView.IsMine || PhotonNetwork.IsMasterClient)) return;

        // Handle drag input
        if (Input.GetMouseButtonDown(0)) // Left mouse button click
        {
            // Check if the mouse is over the parent GameObject (the empty character object)
            if (IsMouseOverCharacter())
            {
                isDragging = true;
                mouseOffset = transform.position - GetMouseWorldPosition();
            }
        }

        if (Input.GetMouseButtonUp(0)) // Left mouse button released
        {
            isDragging = false;
        }

        if (isDragging)
        {
            MoveCharacter();
        }
    }

    // Check if the mouse is over the parent GameObject (empty GameObject) that represents the character
    private bool IsMouseOverCharacter()
    {
        // Raycast to check if the mouse is over the GameObject
        Collider2D collider = GetComponent<Collider2D>();
        return collider != null && collider.OverlapPoint(GetMouseWorldPosition());
    }

    // Convert the mouse position to world position
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = mainCamera.nearClipPlane;
        return mainCamera.ScreenToWorldPoint(mousePos);
    }

    // Update character position
    private void MoveCharacter()
    {
        Vector3 targetPosition = GetMouseWorldPosition() + mouseOffset;

        // For Host (DM), allow direct movement and sync to all players
        if (PhotonNetwork.IsMasterClient)
        {
            // Update position for Host (DM)
            transform.position = Vector3.Lerp(transform.position, targetPosition, dragSpeed * Time.deltaTime);

            // Sync position across the network
            photonView.RPC("SyncPosition", RpcTarget.AllBuffered, transform.position);
        }
        else
        {
            // For non-host players, move their own character and sync across the network
            transform.position = Vector3.Lerp(transform.position, targetPosition, dragSpeed * Time.deltaTime);
            photonView.RPC("SyncPosition", RpcTarget.AllBuffered, transform.position);
        }
    }

    // This RPC is used to sync the character position across all players
    [PunRPC]
    private void SyncPosition(Vector3 newPosition)
    {
        // Only update position for players who don't own the character
        if (!photonView.IsMine)
        {
            // Ensure the position is updated for remote players
            transform.position = newPosition;
        }
        else
        {
            // Ensure the local player sees the correct position
            if (photonView.IsMine)
            {
                transform.position = newPosition;
            }
        }
    }
}