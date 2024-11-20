using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //Variables
    [SerializeField]
    private Transform z_Player;
    [SerializeField]
    private float z_BoundX = 0.1f;
    [SerializeField]
    private float z_BoundY = 0.2f;

    //Methods
    private void LateUpdate()
    {
        FollowPlayer();
    }

    private void FollowPlayer()
    {
        //Create a vector value that will be used to move the camera
        Vector2 moveDirection = Vector2.zero;

        //Calculate the offset of the player according to the Bound
        float deltaX = z_Player.position.x - transform.position.x;
        float deltaY = z_Player.position.y - transform.position.y;

        //X Axis
        //Apply the offset to the created move vector depedning on the direction the player moved
        if (deltaX > z_BoundX || deltaX < -z_BoundX)
        {
            //The player is on the right side
            if (z_Player.position.x > transform.position.x)
            {
                moveDirection.x = deltaX - z_BoundX;
            }
            //The player is on the left side
            else
            {
                moveDirection.x = deltaX + z_BoundX;
            }
        }

        //Y Axis
        //Apply the offset to the created move vector depedning on the direction the player moved
        if (deltaY > z_BoundY || deltaY < -z_BoundY)
        {
            //The player is on the upper side
            if (z_Player.position.y > transform.position.y)
            {
                moveDirection.y = deltaY - z_BoundY;
            }
            //The player is on the lower side
            else
            {
                moveDirection.y = deltaY + z_BoundY;
            }
        }

        //Apply the move vector to the camera position
        transform.position += new Vector3(moveDirection.x, moveDirection.y, 0);
    }
}