using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.ProBuilder.AutoUnwrapSettings;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private bool enableAirStrafing = true;

    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 5f;

    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float gravity = 12f;
    [SerializeField] private float initialFallVelocity = -2f;

    [SerializeField] private float groundAccel = 5f;
    [SerializeField] private float maxGroundSpeed = 5f;
    [SerializeField] private float groundFriction = 2f;

    [SerializeField] private float airAccel = 5f;
    [SerializeField] private float maxAirSpeed = 5f;
    [SerializeField] private float airFriction = 2f;

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
    private float accel;
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


        //direction = wishDir

        //Get direction Vector/Arrow, Clamp it under 1 to prevent faster strafing, and then apply speed
        Vector3 directionUnclamped = Quaternion.Euler(0f, camTransform.eulerAngles.y, 0f) * new Vector3(moveInput.x, 0, moveInput.y);
        Vector3 direction = directionUnclamped.normalized;
        Vector3 targetVelocity = direction * speed;

        //instead of slowly moving towards targetvelocity automatically, you add speed instead until you reach that.
        if (isGrounded)
        {
            GroundAccelerate(direction);
        }
        else
        {
            AirAccelerate(direction);
        }

        //Final movement to apply
        finalMove = currentVelocity;
        finalMove.y = verticalVelocity;

        if (!isGrounded) impulseVelocity = Vector3.Lerp(impulseVelocity, Vector3.zero, 1f - Mathf.Exp(-impulseDecayAir * Time.deltaTime));
        if (isGrounded) impulseVelocity = Vector3.Lerp(impulseVelocity, Vector3.zero, 1f - Mathf.Exp(-impulseDecay * Time.deltaTime));

        characterController.Move((finalMove + impulseVelocity) * Time.deltaTime);

        //Debug.Log($"finalMove: {finalMove}, verticalVelocity: {verticalVelocity}, currentVelocity: {currentVelocity}, targetVelocity: {targetVelocity}, impulseVelocity: {impulseVelocity}, direction: {direction}, directionUnclamped: {directionUnclamped}, speed: {speed}");

    }

    void AirAccelerate(Vector3 direction)
    {
        float currentSpeed = Vector3.Dot(currentVelocity, direction);
        float addSpeed = maxAirSpeed - currentSpeed; //returns how much speed is left allowed, also wont add if result is negative
        if (addSpeed > 0f) //if there is still room to add speed
        {
            accel = airAccel * maxAirSpeed * Time.deltaTime;
            accel = Mathf.Min(addSpeed, accel); //apply the smaller change if any
            currentVelocity += direction * accel;  //(0, 0, 3) + (0, 0, 0.668) = (0, 0, 3.668)
        }

        Vector3 friction = currentVelocity * airFriction * Time.deltaTime;
        currentVelocity -= friction;
        //Debug.Log($"currentVelocity: {currentVelocity}, currentSpeed: {currentSpeed}, direction: {direction}, acceleration: {accel}");
        //Debug.Log($"currentVelocity: {currentVelocity}, currentSpeed: {currentSpeed}, addSpeed: {maxAirSpeed} - {currentSpeed}, accel: {accel}, direction: {direction}");
    }

    void GroundAccelerate(Vector3 direction)
    {

        float currentSpeed = Vector3.Dot(currentVelocity, direction);
        float addSpeed = maxGroundSpeed - currentSpeed; //returns how much speed is left allowed, also wont add if result is negative
        if (addSpeed > 0f) //if there is still room to add speed
        {
            accel = groundAccel * maxGroundSpeed * Time.deltaTime;
            accel = Mathf.Min(addSpeed, accel); //apply the smaller change if any
            currentVelocity += direction * accel;  //(0, 0, 3) + (0, 0, 0.668) = (0, 0, 3.668)
        }


        Vector3 friction = currentVelocity * groundFriction * Time.deltaTime;
        currentVelocity -= friction;

        //Debug.Log($"currentVelocity: {currentVelocity}, currentSpeed: {currentSpeed}, direction: {direction}, acceleration: {accel}");
        //Debug.Log($"currentVelocity: {currentVelocity}, currentSpeed: {currentSpeed}, addSpeed: {maxAirSpeed} - {currentSpeed}, accel: {accel}, direction: {direction}");
    }

    public Vector3 AddImpulse(Vector3 direction, float force)
    {
        Debug.Log("AddImpulse");

        Vector3 impulseAmount = direction * force;

        return impulseAmount;
    }

}
