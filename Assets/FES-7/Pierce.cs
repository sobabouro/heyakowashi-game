using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pierce : MonoBehaviour
{
    [SerializeField]
    private int durabilityRecoveryAmount;

    public int Connect(Breaker breaker)
    {
        this.gameObject.transform.SetParent(breaker.GetContainer());        // 自身の親をBreaker.containerにする
        GameObject container = breaker.GetContainer().gameObject;           
        container.GetComponent<Container>().SetRegisteredObject(this.gameObject);   // Containerクラスの登録オブジェクトを自身にする

        return durabilityRecoveryAmount; // 回復する耐久値を返す
    }

    private Vector3 DecideConnectPosition()
    {
        Vector3 connectPosition = new Vector3();

        return connectPosition;
    }
}
