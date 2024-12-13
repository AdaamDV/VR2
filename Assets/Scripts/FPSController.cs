using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    // We start with the variables
    public Camera playerCamera; //gets the player camera
    public float walkSpeed = 6f; //speed that player walks
    public float runSpeed = 12f; //speed that player runs when pressing key
    public float jumpPower = 7f; //height of jump from player
    public float gravity = 10f; //intensity of gravity

    public float lookSpeed = 2f; //speed of rotation using mouse
    public float lookXLimit = 45f; // limit of looking around?

    Vector3 moveDirection = Vector3.zero; //init of the movement direction
    float rotationX = 0; //init of eventual rotation variable

    public bool canMove = true; //checks if player is/isn't prohibited of moving

    CharacterController characterController; //no clue

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>(); //init the controller
        Cursor.lockState = CursorLockMode.Locked; //lock the cursor at the start? (maybe to always start looking the same way?)
        Cursor.visible = false; //make sure the cursor doesn't show up on the screen.
    }

    // Update is called once per frame
    void Update()
    {
        #region Handles Movement
        Vector3 forward = transform.TransformDirection(Vector3.forward); //move forward and backward
        Vector3 right = transform.TransformDirection(Vector3.right); //move left and right

        // Press Left Shift to run:
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0; //if statement for movement and running, multiplies the speed with current status
        float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0; //same thing for horizontal
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY); //establish the final move direction based on calculations before

        #endregion

        #region Handles Jumping 
        //this is all pretty straight forward
        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        #endregion

        #region Handles Rotation
        characterController.Move(moveDirection * Time.deltaTime); //why move here?

        if(canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);//implement maximum turning speed
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0); //perform the rotation on the camera with euler quaternion
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0); //perform the rotation on the object the same way
        }

        #endregion
    }
}
