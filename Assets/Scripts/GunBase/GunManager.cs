using UnityEngine;

public class GunManager : MonoBehaviour
{

    [SerializeField] private float maxDistance = 100f;

    private GameObject hitObject;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            Fire();
        }
    }

    void Fire()
    {
        Ray gunRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(gunRay, out RaycastHit hitInfo, maxDistance))
        {
            if (hitInfo.collider.gameObject.CompareTag("Enemy"))
            {
                hitObject = hitInfo.collider.gameObject;
                //hitObject.health -= 10;
                Debug.Log(hitObject.GetComponent<Health>().health);
            }
        }
    }
}
