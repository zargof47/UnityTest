using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Launcher : MonoBehaviour
{
    [SerializeField] private InputActionReference fireAction;

    [SerializeField] private Transform camTransform;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Transform firePoint;

    [SerializeField] private GameObject gernade;
    [SerializeField] private float fireForce = 20f;

    [SerializeField] private float fuseTime = 5f;

    private Vector3 addVelocity;

    void Awake()
    {
        Physics.gravity = new Vector3(0, -24f, 0);
    }

    void OnEnable()
    {
        fireAction.action.performed += fireInput;
    }

    void OnDisable()
    {
        fireAction.action.performed -= fireInput;
    }

    void fireInput(InputAction.CallbackContext context)
    {
        GameObject gernadeInstance = Instantiate(gernade, firePoint.position, firePoint.rotation);
        gernadeInstance.GetComponent<Rigidbody>().linearVelocity = addVelocity;
        StartCoroutine(fuse(gernadeInstance, fuseTime));
        
    }
    void FixedUpdate()
    {
        addVelocity = playerController.currentVelocity + camTransform.TransformDirection(Vector3.forward * fireForce);
        //addVelocity = Vector3.Lerp(addVelocity, Vector3.zero, 1f - Mathf.Exp(-5f * Time.fixedDeltaTime));
        
    }

    IEnumerator fuse(GameObject gernadeInstance, float fuseTime)
    {
        yield return new WaitForSeconds(fuseTime);
        gernadeInstance.GetComponent<Explosion>().explode = true;
        yield return new WaitForSeconds(0.1f);
        Destroy(gernadeInstance);
    }
}
