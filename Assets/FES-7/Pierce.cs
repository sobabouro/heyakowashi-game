using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MixedReality.Toolkit.SpatialManipulation;
using UnityEngine.Events;

public class Pierce : MonoBehaviour
{
    [SerializeField]
    private int durabilityRecoveryAmount;
    [SerializeField]
    private int scoreRecoveryAmount;
    [SerializeField]
    private bool canConnect;
    private bool isConnected = false;

    // オブジェクト破壊時に呼び出すイベント登録
    public UnityEvent onBreakEvent;

    // オブジェクト結合時に呼び出すイベント登録
    public UnityEvent onConnectEvent;

    // 結合する座標の設定
    private void DecideConnectPosition()
    {

    }

    // 刺突属性による結合の開始
    public (int, int) Connect(Breaker breaker)
    {
        if (!canConnect)
        {
            // 破壊時に呼び出されるイベントを呼び出す
            onBreakEvent?.Invoke();
            return (0, 0);
        }
            
        // 既に結合しているオブジェクトに対して、刺突属性で再び壊した場合
        if(isConnected)
        {
            isConnected = false;
            Physics.IgnoreCollision(this.gameObject.GetComponent<Collider>(), breaker.gameObject.GetComponent<Collider>(), false);

            // 破壊時に呼び出されるイベントを呼び出す
            onBreakEvent?.Invoke();

            return (0, 0);
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

        // 破壊時に呼び出されるイベントを呼び出す
        onConnectEvent?.Invoke();

        // 回復する耐久値を返す
        return (durabilityRecoveryAmount, scoreRecoveryAmount);
    }
}