using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] public PlayerController playerController;

    [SerializeField] private Transform mainCamera;
    [SerializeField] private Transform headPos;

    [SerializeField] private float sensitivity = 2f;
    private float verticalRot;

    [SerializeField] private float maxTiltAngle = 4f;
    private float horizontalInput;

    //Mouse Inputs
    private float inputX;
    private float inputY;

    //Roll Calculation
    private float currentRoll;
    private float finalRoll;

    void Update()
    {
        MouseInput();
        CameraSway();
    }

    void MouseInput()
    {
        inputX = Input.GetAxis("Mouse X") * sensitivity;
        inputY = Input.GetAxis("Mouse Y") * sensitivity;

        verticalRot -= inputY;
        verticalRot = Mathf.Clamp(verticalRot, -90f, 90f);

        mainCamera.localEulerAngles = new Vector3(verticalRot, 0f, finalRoll);
        transform.Rotate(Vector3.up * inputX);
    }

    void CameraSway()
    {
        //Calculate desired roll
        horizontalInput = Mathf.Clamp(inputX, -3f, 3f);
        currentRoll = mainCamera.localEulerAngles.z;
        float targetRoll = -horizontalInput * maxTiltAngle;

        //Smoothly set desired roll
        finalRoll = Mathf.LerpAngle(currentRoll, targetRoll, 1f - Mathf.Exp(-5f * Time.deltaTime));

        Debug.Log(new Vector2(horizontalInput, targetRoll));

        Debug.DrawRay(transform.position, transform.right * 2f, Color.red);
        Debug.DrawRay(transform.position, -transform.right * 2f, Color.red);
    }
}
