using UnityEngine;
using System.Collections;

public class ThrowRang : MonoBehaviour
{
    [SerializeField] GameObject rang;
    [SerializeField] int maxDistance = 100;

    public bool isThrown = false;
    public float throwTime = 2f;

    private Vector3 currentPos;
    private Vector3 throwPos;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.E) && !isThrown)
        {
            currentPos = rang.transform.position;
            ThrowRay();
        }
    }

    void ThrowRay()
    {
        Ray throwRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(throwRay, out RaycastHit hitInfo, maxDistance))
        {
            throwPos = hitInfo.point;
            isThrown = true;
            StartCoroutine(Throwroutine());

            Debug.Log(throwPos.ToString());
        }
    }

    IEnumerator Throwroutine()
    {
        Debug.Log("Throwing");

        float t = 0f;

        //while (t<1)
        //{
            //rang.transform.position = Vector3.Lerp(currentPos, throwPos, throwTime);
            //t += Time.deltaTime;
        //yield return null;
        //}
        rang.transform.position = Vector3.Lerp(currentPos, throwPos, throwTime);
        Debug.Log("done");
        yield return null;
    }
}
