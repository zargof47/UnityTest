using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class Explosion : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private PlayerController playerController;

    [SerializeField] private float maxRange = 10f;
    [SerializeField] private float force = 20f;

    private Vector3 direction;
    private float distance;
    private float t;

    void Update()
    {
        direction = (player.transform.position - transform.position).normalized;
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            Explode();
            Debug.Log("E pressed");
        }
    }

    void Explode()
    {
        float appliedForce = force;

        distance = Vector3.Distance(transform.position, player.transform.position);
        t = Mathf.InverseLerp(maxRange, 0f, distance);
        appliedForce *= t;

        Debug.Log($"proximity percent: {t}%, distance: {distance}, appliedForce: {appliedForce}");

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, maxRange))
        {
            Debug.DrawRay(transform.position, direction * maxRange, Color.green);
            if (hit.collider == player.GetComponent<Collider>())
            {
                Debug.Log("Hit Player: " + direction + hit.collider);
                playerController.impulseVelocity = playerController.AddImpulse(direction, appliedForce);
            }
            else
            {
                Debug.Log("Hit Something: " + direction + hit.collider);
                playerController.impulseVelocity = Vector3.zero;
            }
        }
    }
}

/*origin: Where the ray starts.
direction: Which way it points.
hit: Information about whatever it hits, including hit.point and hit.collider.
distance: How far the ray checks.*/