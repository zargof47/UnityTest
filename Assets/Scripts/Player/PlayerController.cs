using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.ProBuilder.AutoUnwrapSettings;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private bool enableAirStrafing = true;

    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float addedSpeed = 5f;

    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float gravity = 12f;
    [SerializeField] private float initialFallVelocity = -2f;

    [SerializeField] private float response = 20f;

    [SerializeField] private Transform camTransform;
    [SerializeField] private CharacterController characterController;

    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference sprintAction;

    public Vector2 moveInput;
    private Vector3 currentVelocity;
    private Vector3 finalMove;
    public Vector3 impulseVelocity;

    private float speed;
    private bool isGrounded;
    private bool isSprinting = false;
    private float verticalVelocity; 

    void OnEnable()
    {
        moveAction.action.performed += StoreMovementInput;
        moveAction.action.canceled += StoreMovementInput;

        jumpAction.action.performed += JumpInput;

        sprintAction.action.performed += SprintInput;
        sprintAction.action.canceled += SprintInput;


    }
    void OnDisable()
    {
        moveAction.action.performed -= StoreMovementInput;
        moveAction.action.canceled -= StoreMovementInput;

        jumpAction.action.performed -= JumpInput;

        sprintAction.action.performed -= SprintInput;
        sprintAction.action.canceled -= SprintInput;

    }





    void Update()
    {
        isGrounded = characterController.isGrounded;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; 

        HandleGravity();
        HandleMovement();
    }





    void StoreMovementInput(InputAction.CallbackContext context)
    {
        //Raw move input
        moveInput = context.ReadValue<Vector2>();
    }

    void JumpInput(InputAction.CallbackContext context)
    {
        //Jump check
        if (isGrounded)
        {
            verticalVelocity = jumpForce;
        }
    }

    void SprintInput(InputAction.CallbackContext context)
    {
        //Allow sprinting
        isSprinting = context.performed;
        //Debug.Log(context.phase);
    }





    void HandleGravity()
    {
        //Constantly pull the player continously when falling without jumping
        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = initialFallVelocity;
        }

        //when not grounded, (Jumping) pull the player down
        verticalVelocity -= gravity * Time.deltaTime;
    }

    void HandleMovement()
    {
        //Set Speed
        speed = walkSpeed;
        if (isSprinting && isGrounded)
        {
            speed = runSpeed;
        }
        else if (!isSprinting && isGrounded)
        {
            speed = walkSpeed;
        }
        else if (isSprinting && !isGrounded)
        {
            speed = runSpeed + addedSpeed;
        }
        else if (!isSprinting && !isGrounded)
        {
            speed = walkSpeed + addedSpeed;
        }

        //Get direction Vector/Arrow, Clamp it under 1 to prevent faster strafing, and then apply speed
        Vector3 direction = camTransform.TransformDirection(new Vector3(moveInput.x, 0, moveInput.y));
        direction = Vector3.ClampMagnitude(direction, 1f);
        Vector3 targetVelocity = direction * speed;
        
        //smoothen movement by lerping
        if (!enableAirStrafing)
        {
            if (isGrounded) currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, 1f - Mathf.Exp(-response * Time.deltaTime));
        }
        else
        {
            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, 1f - Mathf.Exp(-response * Time.deltaTime));
        }

        //Final movement to apply
        finalMove = currentVelocity;
        finalMove.y = verticalVelocity;

        //Debug.Log(finalMove + " " + currentVelocity + " " + speed + " " + isGrounded);
        //if (Input.GetKeyDown(KeyCode.E)) AddImpulse(finalMove);
        characterController.Move((finalMove + impulseVelocity) * Time.deltaTime);
        
    }

    public Vector3 AddImpulse(Vector3 direction)
    {
        Debug.Log("AddImpulse");

        Vector3 impulseAmount = direction * 10f;

        return impulseAmount;
    }

}
