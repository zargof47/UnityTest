using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.ProBuilder.AutoUnwrapSettings;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private bool enableAirStrafing = true;

    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float maxAirSpeed = 5f;

    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float gravity = 12f;
    [SerializeField] private float initialFallVelocity = -2f;

    [SerializeField] private float groundAccel = 10f;
    [SerializeField] private float airAccel = 5f;

    [SerializeField] private float impulseDecayAir = 5f;
    [SerializeField] private float impulseDecay = 5f;

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
        if (isSprinting)
        {
            speed = runSpeed;
        }
        else if (!isSprinting)
        {
            speed = walkSpeed;
        }




        //Get direction Vector/Arrow, Clamp it under 1 to prevent faster strafing, and then apply speed
        Vector3 directionUnclamped = Quaternion.Euler(0f, camTransform.eulerAngles.y, 0f) * new Vector3(moveInput.x, 0, moveInput.y);
        Vector3 direction = directionUnclamped.normalized;
        Vector3 targetVelocity = direction * speed;

        if (isGrounded)
        {
            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, 1f - Mathf.Exp(-groundAccel * Time.deltaTime));
        }
        else
        {
            currentVelocity += direction * airAccel * Time.deltaTime;
        }
        float wishSpeed = currentVelocity.magnitude;
        float currentSpeed = Vector3.Dot(currentVelocity, direction);
        float addSpeed = maxAirSpeed - currentSpeed; //returns how much speed is needed

        //Debug.Log($"direction: {direction}, currentVelocity; {currentVelocity}, totalSpeed: {totalSpeed}, wishSpeed: {wishSpeed}");
        Debug.Log($"wishSpeed: {wishSpeed}, currentSpeed: {currentSpeed}");





        //Final movement to apply
        finalMove = currentVelocity;
        finalMove.y = verticalVelocity;

        if (!isGrounded) impulseVelocity = Vector3.Lerp(impulseVelocity, Vector3.zero, 1f - Mathf.Exp(-impulseDecayAir * Time.deltaTime));
        if (isGrounded) impulseVelocity = Vector3.Lerp(impulseVelocity, Vector3.zero, 1f - Mathf.Exp(-impulseDecay * Time.deltaTime));

        characterController.Move((finalMove + impulseVelocity) * Time.deltaTime);

        //Debug.Log($"finalMove: {finalMove}, verticalVelocity: {verticalVelocity}, currentVelocity: {currentVelocity}, targetVelocity: {targetVelocity}, impulseVelocity: {impulseVelocity}, direction: {direction}, directionUnclamped: {directionUnclamped}, speed: {speed}");

    }


    public Vector3 AddImpulse(Vector3 direction, float force)
    {
        Debug.Log("AddImpulse");

        Vector3 impulseAmount = direction * force;

        return impulseAmount;
    }

}
