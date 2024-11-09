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
        // Allow drag only for the local owner or MasterClient
        if (!(photonView.IsMine || PhotonNetwork.IsMasterClient)) return;

        // Handle drag input
        if (Input.GetMouseButtonDown(0) && IsMouseOverCharacter()) // Left mouse button click
        {
            isDragging = true;
            mouseOffset = transform.position - GetMouseWorldPosition();
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

    // Check if the mouse is over the character's GameObject
    private bool IsMouseOverCharacter()
    {
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

    // Update character position and sync across the network
    private void MoveCharacter()
    {
        Vector3 targetPosition = GetMouseWorldPosition() + mouseOffset;

        // Move the character
        transform.position = Vector3.Lerp(transform.position, targetPosition, dragSpeed * Time.deltaTime);

        // Sync the position across the network
        photonView.RPC("SyncPosition", RpcTarget.AllBuffered, transform.position);
    }

    // RPC to sync the character position across all players
    [PunRPC]
    private void SyncPosition(Vector3 newPosition)
    {
        // If the player receiving the RPC is not the owner and not the MasterClient, update position
        if (!photonView.IsMine || PhotonNetwork.IsMasterClient)
        {
            transform.position = newPosition;
        }
    }
}
