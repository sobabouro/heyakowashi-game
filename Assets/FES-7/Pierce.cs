using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pierce : MonoBehaviour
{
    [SerializeField]
    private int durabilityRecoveryAmount;

    public void Connect(Breaker breaker)
    {
        this.gameObject.transform.parent = breaker.GetContainer();
    }

    private Vector3 DecideConnectPosition()
    {
        Vector3 connectPosition = new Vector3();

        return connectPosition;
    }
}
