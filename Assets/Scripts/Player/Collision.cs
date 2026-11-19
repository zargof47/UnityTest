using UnityEngine;

public class Collision : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Collider collider;

    private int maxBounces = 5;
    private float skinWidth = 0.015f;

    private Vector3 CollideAndSlide(Vector3 velocity, Vector3 position, int depth, Vector3 currentVelocity)
    {
        if (depth >= maxBounces)
        {
            return Vector3.zero;
        }

        float distance = velocity.magnitude + skinWidth;

        RaycastHit hit;
        if (Physics.SphereCast(position, collider.bounds.extents.x, velocity.normalized, out hit, distance))
        {
            Vector3 reducedVelocity = velocity.normalized * (hit.distance - skinWidth);
            Vector3 leftoverVelocity = velocity - reducedVelocity;

            //allow room for collision check
            if (reducedVelocity.magnitude <= skinWidth)
            {
                reducedVelocity = Vector3.zero;
                leftoverVelocity = ProjectAndScale(leftoverVelocity, hit.normal);
            }
            else
            { //scale the velocity by the surface angle and the currentVelocity
                float scale = 1 - Vector3.Dot(new Vector3(hit.normal.x, 0f, hit.normal.z).normalized, -new Vector3(currentVelocity.x, 0f, currentVelocity.z).normalized);
                leftoverVelocity = ProjectAndScale(leftoverVelocity, hit.normal) * scale;
            }

            //reducedVelocity + next level of recursion
            return reducedVelocity + CollideAndSlide(leftoverVelocity, position + reducedVelocity, depth + 1, currentVelocity);
        }
        return velocity;
    }

    private Vector3 ProjectAndScale(Vector3 velocity, Vector3 normal)
    {
        float mag = velocity.magnitude;
        velocity = Vector3.ProjectOnPlane(velocity, normal).normalized;
        velocity *= mag;

        return velocity;
    }
}
