// Usage: this script is meant to be placed on the player.
// The player must have a CharacterController component.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private CharacterController charController;

    private Vector2 inputVec;                   // Horizontal movement input
    private bool jump;                          // Whether the jump key is inputted
    private bool sprintKeyDown = false;
    private bool sprintKeyUp = false;
    
    private Vector3 moveVec = Vector3.zero;     // Vector3 used to move the character controller
    private float moveSpeed;
    
    private bool isJumping = false;             // Player has jumped and not been grounded yet
    private bool groundedLastFrame = false;     // Player was grounded during the last frame
    private bool isSprinting = false;

    [SerializeField] private float gravityMultiplier = 1f;
    [SerializeField] private float stickToGroundForce = 0.1f;
    [SerializeField] private float friction = 5f;

    [SerializeField] private float jumpSpeed = 5f; // Initial upwards speed of the jump

    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float minAirSpeed = 3f;
    [SerializeField] private float sprintSpeed = 7f;

    [SerializeField] private float airControlRatio = 0.02f;
    [SerializeField] private float groundControlRatio = 0.1f;

    void Awake()
    {
        this.charController = GetComponent<CharacterController>();
        if (charController == null)
            Debug.LogError(GetType() + ": The character controller component is not initialized.");
        
        this.moveSpeed = this.walkSpeed;
    }

    void Update()
    {
        // Handle inputs first
        float up = InputManager.GetKey("Strafe Up") ? 1 : 0,
        left = InputManager.GetKey("Strafe Left") ? -1 : 0,
        down = InputManager.GetKey("Strafe Down") ? -1 : 0,
        right = InputManager.GetKey("Strafe Right") ? 1 : 0;

        this.inputVec = new Vector2(left + right, up + down);
        if (this.inputVec.magnitude > 1)
            this.inputVec = this.inputVec.normalized; // Normalize the input vector

        if (!this.jump)
            this.jump = InputManager.GetKeyDown("Jump");
        
        this.sprintKeyDown = (InputManager.GetKeyDown("Sprint") || InputManager.GetKeyDown("Strafe Up"))
            && (InputManager.GetKey("Sprint") && InputManager.GetKey("Strafe Up"));
        this.sprintKeyUp = InputManager.GetKeyUp("Sprint") || InputManager.GetKeyUp("Strafe Up");

        // Sprint
        if (this.sprintKeyDown && !this.isSprinting)
        {
            this.isSprinting = true;
            this.moveSpeed = this.sprintSpeed;
        }
        else if (this.sprintKeyUp && this.isSprinting)
        {
            this.isSprinting = false;
            this.moveSpeed = this.walkSpeed;
        }

        // Jump
        if (!this.groundedLastFrame && this.charController.isGrounded)
        {
            this.moveVec.y = 0f;
            this.isJumping = false;
        }

        // The following two lines of code are not be needed if charController.isGrounded is set during charController.Move().
        // TODO: Try to find this piece of info in Unity's documentation.
        if (!this.charController.isGrounded && !this.isJumping && this.groundedLastFrame)
            this.moveVec.y = 0f;

        if (this.charController.isGrounded)
            GroundMove();
        else
            AirMove();
        
        this.charController.Move(this.moveVec * Time.deltaTime);
        this.jump = false;
        this.groundedLastFrame = this.charController.isGrounded;
    }

    void GroundMove()
    {
        Vector3 wishVel = transform.forward * this.inputVec.y + this.transform.right * this.inputVec.x;
        Vector3 wishDir = wishVel.normalized;
        Vector3 prevMove = new Vector3(this.moveVec.x, 0f, this.moveVec.z);

        // Calculate movement vector to add
        Vector3 addMove;
        if (this.groundControlRatio * prevMove.magnitude > 1f)
            addMove = wishDir * this.groundControlRatio * prevMove.magnitude;
        else
            addMove = wishDir * this.groundControlRatio * this.moveSpeed;

        // Apply friction to previous move
        float prevSpeed = prevMove.magnitude;
        if (prevSpeed != 0) // To avoid divide by zero errors
        {
            float drop = prevSpeed * this.friction * Time.deltaTime;
            float newSpeed = prevSpeed - drop;
            if (newSpeed < 0)
                newSpeed = 0;
            else if (newSpeed != prevSpeed)
            {
                newSpeed /= prevSpeed;
                prevMove = prevMove * newSpeed;
            }
        }

        // The next move is the previous move plus an input-based movement vector
        Vector3 nextMove = prevMove + addMove;
        if (nextMove.magnitude > this.moveSpeed)
            nextMove = nextMove.normalized * this.moveSpeed;

        // y-component is calculated separately
        nextMove.y = -this.stickToGroundForce;
        if (this.jump)
        {
            nextMove.y = this.jumpSpeed;
            this.jump = false;
            this.isJumping = true;
        }

        this.moveVec = nextMove;
    }

    void AirMove()
    {
        Vector3 wishVel = this.transform.forward * this.inputVec.y + transform.right * this.inputVec.x;
        Vector3 wishDir = wishVel.normalized;
        Vector3 prevMove = new Vector3(this.moveVec.x, 0f, this.moveVec.z);

        // Calculate movement vector to add
        Vector3 addMove;
        if (this.airControlRatio * prevMove.magnitude > this.airControlRatio * this.minAirSpeed)
            addMove = wishDir * this.airControlRatio * prevMove.magnitude;
        else
            addMove = wishDir * this.airControlRatio * this.minAirSpeed;

        // The next move is the previous move plus an input-based movement vector
        Vector3 nextMove = prevMove + addMove;
        if (nextMove.magnitude > this.moveSpeed)
            nextMove = nextMove.normalized * this.moveSpeed;

        // y-component is calculated separately
        nextMove.y = this.moveVec.y;
        nextMove += Physics.gravity * this.gravityMultiplier * Time.deltaTime;

        this.moveVec = nextMove;
    }

    // Called during CharacterController.Move()
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // If this frame's attempted move was blocked by a collider, project the movement vector onto the collider's surface
        Vector3 surfaceNormal = hit.normal;
        if (Vector3.Dot(surfaceNormal, this.moveVec) < 0)
            this.moveVec = Vector3.ProjectOnPlane(this.moveVec, surfaceNormal);
    }
}
