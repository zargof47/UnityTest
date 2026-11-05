using UnityEngine;

public class Explode : MonoBehaviour
{
    public float explodeForce = 10;
    public Rigidbody rb;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            ExplodeBomb();
        }
    }

    private void ExplodeBomb()
    {
        rb.AddExplosionForce(explodeForce, transform.position, 10);
    }
}
