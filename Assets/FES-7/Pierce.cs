using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pierce : MonoBehaviour
{
    [SerializeField]
    private int durabilityRecoveryAmount;

    // 刺突属性による結合の開始
    public int Connect(Breaker breaker)
    {
        this.gameObject.transform.SetParent(breaker.GetContainer());        // 自身の親をBreaker.containerにする
        GameObject container = breaker.GetContainer().gameObject;           
        container.GetComponent<Container>().SetRegisteredObject(this.gameObject);   // Containerクラスの登録オブジェクトを自身にする
        breaker.enabled = false;

        return durabilityRecoveryAmount; // 回復する耐久値を返す
    }

    // 結合する座標の設定
    private Vector3 DecideConnectPosition()
    {
        Vector3 connectPosition = new Vector3();

        return connectPosition;
    }
}
