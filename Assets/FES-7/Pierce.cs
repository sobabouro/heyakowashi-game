using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MixedReality.Toolkit.SpatialManipulation;

public class Pierce : MonoBehaviour
{
    [SerializeField]
    private int durabilityRecoveryAmount;

    // 刺突属性による結合の開始
    public int Connect(Container container)
    {
        // コンテナの子オブジェクトにされるrigidbodyの破棄
        Rigidbody rigidbody = this.gameObject.GetComponent<Rigidbody>();
        Destroy(rigidbody);

        // 自身の親をBreaker.containerにする
        this.gameObject.transform.SetParent(container.gameObject.transform);

        // Containerクラスの登録オブジェクトを自身にする
        container.SetRegisteredObject(this.gameObject);

        // 回復する耐久値を返す
        return durabilityRecoveryAmount;
    }

    // 結合する座標の設定
    private void DecideConnectPosition()
    {

    }
}