using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class Explosion : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private PlayerController playerController;

    [SerializeField] private float hitRadius = 10f;

    private Vector3 direction;

    void Update()
    {
        direction = (player.transform.position - transform.position).normalized;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Explode();
            Debug.Log("e");
        }
    }

    void Explode()
    {
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, hitRadius))
        {
            Debug.DrawRay(transform.position, direction * hitRadius, Color.green);
            if (hit.collider == player.GetComponent<Collider>())
            {
                Debug.Log("Hit Player: " + direction + hit.collider);
                playerController.impulseVelocity = playerController.AddImpulse(direction);
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