using UnityEngine;

public class RangAttach : MonoBehaviour
{

    [SerializeField] ThrowRang throwRang;
    [SerializeField] private Transform grabpoint;
    void Update()
    {
        if (!throwRang.isThrown)
        {
            transform.position = grabpoint.position;
        }
    }
}
