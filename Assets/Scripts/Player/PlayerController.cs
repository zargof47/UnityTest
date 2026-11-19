using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder;
using static UnityEngine.ProBuilder.AutoUnwrapSettings;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private bool enableAirStrafing = true;

    [SerializeField] private float walkSpeed = 12f;
    [SerializeField] private float runSpeed = 16f;
    [SerializeField] private float wishCap = 4f;

    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float gravity = 12f;
    [SerializeField] private float initialFallVelocity = -2f;

    [SerializeField] private float groundAccel = 5f;
    [SerializeField] private float maxGroundSpeed = 5f;
    [SerializeField] private float groundFriction = 2f;

    [SerializeField] private float airAccel = 10f;
    [SerializeField] private float maxAirSpeed = 5f;
    [SerializeField] private float airFriction = 2f;

    [SerializeField] private float impulseDecayAir = 5f;
    [SerializeField] private float impulseDecay = 5f;

    [SerializeField] private Transform camTransform;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Collider collider;

    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference sprintAction;

    [SerializeField] private TextMeshProUGUI textMeshProUGUI;

    public Vector2 moveInput;
    public Vector3 currentVelocity; 
    private Vector3 finalMove;
    public Vector3 impulseVelocity;

    private float speed;
    private bool isGrounded;
    private bool isSprinting = false;
    private float verticalVelocity;



    private int maxBounces = 5;
    private float skinWidth = 0.015f;
    private float maxSlopeAngle = 55;

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

    void Awake()
    {
        skinWidth = characterController.skinWidth;
        collider.bounds.Expand(-2 * skinWidth);
    }



    void Update()
    {
        isGrounded = characterController.isGrounded;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; 

        HandleGravity();
        HandleMovement();

        //textMeshProUGUI.text = Mathf.RoundToInt(characterController.velocity.magnitude).ToString();
        textMeshProUGUI.text = ($"{Mathf.RoundToInt(verticalVelocity)}, {Mathf.RoundToInt(new Vector3(characterController.velocity.x, 0f, characterController.velocity.z).magnitude)}");
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
            impulseVelocity.y = 0f;
        }

        //when not grounded, (Jumping) pull the player down
        verticalVelocity -= gravity * Time.deltaTime;
    }

    void HandleMovement()
    {
        //Set Speed
        speed = walkSpeed;
        /*if (isSprinting)
        {
            speed = runSpeed;
        }
        else if (!isSprinting)
        {
            speed = walkSpeed;
        }*/


        //direction = wishDir

        //Get direction Vector/Arrow, Clamp it under 1 to prevent faster strafing, and then apply speed
        Vector3 directionUnclamped = Quaternion.Euler(0f, camTransform.eulerAngles.y, 0f) * new Vector3(moveInput.x, 0, moveInput.y);
        //Vector3 directionUnclamped = camTransform.TransformDirection(new Vector3(moveInput.x, 0, moveInput.y));
        Vector3 direction = directionUnclamped.normalized; // makes it 1

        //instead of slowly moving towards targetvelocity automatically, you add speed instead until you reach that.
        if (isGrounded)
        {
            GroundAccelerate(direction);
        }
        else
        {
            AirAccelerate(direction);
        }

        Debug.DrawRay(transform.position, currentVelocity, Color.pink);
        Debug.DrawRay(transform.position, finalMove, Color.blue);

        Vector3 frameMove = currentVelocity * Time.deltaTime;

        if (Physics.Raycast(transform.position, frameMove, out RaycastHit hit, direction.magnitude))
        {
            Vector3 baseVector = frameMove.normalized * (hit.distance - skinWidth);
            float normalSpeed = Vector3.Dot(currentVelocity, hit.normal);
            if (normalSpeed < 0f)
            {
                currentVelocity += hit.normal * -normalSpeed;
            }

            Debug.DrawRay(hit.point, baseVector, Color.blue);
        }



            //Final movement to apply
            finalMove = currentVelocity + impulseVelocity;
            finalMove.y = verticalVelocity + impulseVelocity.y;

            if (!isGrounded) impulseVelocity = Vector3.Lerp(impulseVelocity, Vector3.zero, 1f - Mathf.Exp(-impulseDecayAir * Time.deltaTime));
            if (isGrounded) impulseVelocity = Vector3.Lerp(impulseVelocity, Vector3.zero, 1f - Mathf.Exp(-impulseDecay * Time.deltaTime));

            characterController.Move((finalMove) * Time.deltaTime);

        }

        void AirAccelerate(Vector3 direction)
        {
            float wishSpeed = direction.magnitude * speed;
            maxAirSpeed = wishSpeed;
            if (wishSpeed > 2f) wishSpeed = 2f;

            float currentSpeed = Vector3.Dot(currentVelocity, direction); //direction = wishV
            float addSpeed = wishSpeed - currentSpeed; //returns how much speed is left allowed, also wont add if result is negative
            if (addSpeed > 0f) //if there is still room to add speed
            {
                float accel = maxAirSpeed * airAccel * Time.deltaTime;
                if (accel > addSpeed) accel = addSpeed;
                accel = Mathf.Min(addSpeed, accel); //apply the smaller change if any
                currentVelocity += direction * accel; //add to velocity
            }

            //Debug.Log($"CurrentVelocity: {currentSpeed}, {currentVelocity}, {finalMove}");
        }

        void GroundAccelerate(Vector3 direction)
        {

            float wishSpeed = direction.magnitude * speed;
            maxGroundSpeed = wishSpeed;
            if (wishSpeed > 2f) wishSpeed = 2f;

            float currentSpeed = Vector3.Dot(currentVelocity, direction); //direction = wishV
            float addSpeed = maxGroundSpeed - currentSpeed; //returns how much speed is left allowed, also wont add if result is negative
            if (addSpeed > 0f) //if there is still room to add speed
            {
                float accel = maxGroundSpeed * groundAccel * Time.deltaTime;
                if (accel > addSpeed) accel = addSpeed;
                accel = Mathf.Min(addSpeed, accel); //apply the smaller change if any
                currentVelocity += direction * accel; //add to velocity
            }


            Vector3 friction = currentVelocity * groundFriction * Time.deltaTime;
            currentVelocity -= friction;

            //Debug.Log($"CurrentVelocity: {currentSpeed}, {currentVelocity}, {finalMove}");
        }

    public Vector3 AddImpulse(Vector3 direction, float force)
    {
        Debug.Log("AddImpulse");

        Vector3 impulseAmount = direction * force;

        return impulseAmount;
    }




    /*private Vector3 CollideAndSlide(Vector3 velocity, Vector3 position, int depth, bool gravityPass, Vector3 initialVelocity)
    {
        if (depth >= maxBounces)
        {
            return Vector3.zero;
        }

        float distance = velocity.magnitude + skinWidth;

        RaycastHit hit;
        if (Physics.SphereCast(position, collider.bounds.extents.x, velocity.normalized, out hit, distance))
        {
            Vector3 snapToSurface = velocity.normalized * (hit.distance - skinWidth);
            Vector3 leftoverVelocity = velocity - snapToSurface;
            float angle = Vector3.Angle(Vector3.up, hit.normal);

            if (snapToSurface.magnitude <= skinWidth)
            {
                snapToSurface = Vector3.zero;
            }

            if (angle < maxSlopeAngle)
            {
                if (gravityPass)
                {
                    return snapToSurface;
                }
                leftoverVelocity = ProjectAndScale(leftoverVelocity, hit.normal);
            }
            else
            {
                float scale = 1 - Vector3.Dot(
                    new Vector3(hit.normal.x, 0, hit.normal.z).normalized,
                    -new Vector3(initialVelocity.x, 0, initialVelocity.z).normalized
                    );

                leftoverVelocity = ProjectAndScale(leftoverVelocity, hit.normal) * scale;
            }


            return snapToSurface + CollideAndSlide(leftoverVelocity, position + snapToSurface, depth + 1, gravityPass, initialVelocity);
        }

        return velocity;
    }

    private Vector3 ProjectAndScale(Vector3 vector, Vector3 normal)
    {
        float mag = vector.magnitude;
        vector = Vector3.ProjectOnPlane(vector, normal).normalized;
        vector *= mag;
        return vector;
    }*/

}
