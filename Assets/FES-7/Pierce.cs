using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MixedReality.Toolkit.SpatialManipulation;

public class Pierce : MonoBehaviour
{
    [SerializeField]
    private int durabilityRecoveryAmount;
    [SerializeField]
    private bool canConnect;
    private bool isConnected = false;

    // 結合する座標の設定
    private void DecideConnectPosition()
    {

    }

    // 刺突属性による結合の開始
    public int Connect(Breaker breaker)
    {
        if (!canConnect) return 0;
        // 既に結合しているオブジェクトに対して、刺突属性で再び壊した場合
        if(isConnected)
        {
            isConnected = false;
            Physics.IgnoreCollision(this.gameObject.GetComponent<Collider>(), breaker.gameObject.GetComponent<Collider>(), false);
            return 0;
        }

        /*
                // ここで結合するオブジェクトの座標を調整する
        */

        // オブジェクトの動きの依存対象の設定
        FixedJoint fixedJoint;
        if (this.gameObject.GetComponent<FixedJoint>() == null)
        {
            fixedJoint = this.gameObject.AddComponent<FixedJoint>();
        }
        else
        {
            fixedJoint = this.gameObject.GetComponent<FixedJoint>();
        }
        fixedJoint.connectedBody = breaker.GetRigidbody();

        isConnected = true;

        // 結合したオブジェクト間の衝突判定の無効化
        Physics.IgnoreCollision(this.gameObject.GetComponent<Collider>(), breaker.gameObject.GetComponent<Collider>(), true);

        // 回復する耐久値を返す
        return durabilityRecoveryAmount;
    }
}