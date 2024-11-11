using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MixedReality.Toolkit.SpatialManipulation;
using UnityEngine.Events;

public class Pierce : MonoBehaviour
{
    [SerializeField, Tooltip("回復耐久値")]
    private int durabilityRecoveryAmount;
    [SerializeField, Tooltip("回復スコア")]
    private int scoreRecoveryAmount;
    [SerializeField, Tooltip("結合可能？")]
    private bool canConnect;
    // 結合している？
    private bool isConnected = false;

    // オブジェクト破壊時に呼び出すイベント登録
    public UnityEvent onBreakEvent;
    // オブジェクト結合時に呼び出すイベント登録
    public UnityEvent onConnectEvent;

    // 結合する座標の設定
    private void DecideConnectPosition()
    {

    }

    /// <summary>
    /// 刺突属性による結合の開始
    /// </summary>
    /// <param name="breaker">壊すものクラス</param>
    /// <returns>回復する耐久値、スコア</returns>
    public (int, int) Connect(Breaker breaker)
    {
        // コライダーの取得
        Collider breakerCollider = breaker.gameObject.GetComponent<Collider>();
        Collider myCollider = this.gameObject.GetComponent<Collider>();

        // 結合できないオブジェクトの場合
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
            // 結合したオブジェクト間の衝突判定の有効化
            Physics.IgnoreCollision(myCollider, breakerCollider, false);

            // 破壊時に呼び出されるイベントを呼び出す
            onBreakEvent?.Invoke();

            return (0, 0);
        }

        /*
                // ここで結合するオブジェクトの座標を調整する
        */

        // オブジェクトの動きの依存対象の設定
        FixedJoint fixedJoint = this.gameObject.GetComponent<FixedJoint>(); ;
        if (fixedJoint == null)
        {
            fixedJoint = this.gameObject.AddComponent<FixedJoint>();
        }
        fixedJoint.connectedBody = breaker.GetRigidbody();

        isConnected = true;

        // 結合したオブジェクト間の衝突判定の無効化
        Physics.IgnoreCollision(myCollider, breakerCollider, true);

        // 結合時に呼び出されるイベントを呼び出す
        onConnectEvent?.Invoke();

        // 回復する耐久値を返す
        return (durabilityRecoveryAmount, scoreRecoveryAmount);
    }
}