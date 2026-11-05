using UnityEngine;

public class CameraEffects : MonoBehaviour
{
    [SerializeField] private CharacterController controller;

    [SerializeField] private bool enable = true;

    [SerializeField] private Transform mainCamera;
    [SerializeField] private Transform headPos;

    [SerializeField] float amplitude = 0.015f;
    [SerializeField] float frequency = 10f;

    private Vector3 startPos;
    private float toggleSpeed = 10f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        startPos = mainCamera.localPosition;
    }

    private void CheckMotion()
    {
        float speed = new Vector3(controller.velocity.x, 0, controller.velocity.z).magnitude;

        if (speed > toggleSpeed) return;

        PlayMotion(FootStepMotion());
    }

    private void PlayMotion(Vector3 motion)
    {
        mainCamera.localPosition += motion;
    }

    private void ResetPosition()
    {
        if (mainCamera.localPosition == startPos) return;
        mainCamera.localPosition = Vector3.Lerp(mainCamera.localPosition, startPos, 1 * Time.deltaTime);
    }

    private Vector3 FootStepMotion()
    {
        Vector3 pos = Vector3.zero;

        pos.y += Mathf.Sin(Time.time * frequency) * amplitude;
        pos.x += Mathf.Cos(Time.time * frequency /2) * amplitude * 2;
        return pos;
    }

    private Vector3 FocusTarget()
    {
        Vector3 pos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        pos = headPos.forward * 15f;
        return pos;
    }

    void Update()
    {
        if (!enabled) return;

        CheckMotion();
        ResetPosition();
        mainCamera.LookAt(FocusTarget());

    }
}
