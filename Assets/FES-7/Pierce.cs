using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MixedReality.Toolkit.SpatialManipulation;
using UnityEngine.Events;
using System;

public class Pierce : MonoBehaviour
{
    [SerializeField, Tooltip("回復耐久値")]
    private int durabilityRecoveryAmount;
    [SerializeField, Tooltip("回復スコア")]
    private int scoreRecoveryAmount;
    [SerializeField, Tooltip("結合可能？")]
    private bool canConnect;
    // 結合している？
    private bool isConnect = false;
    // 刺突結合している時の結合相手のオブジェクト
    private List<Collider> connectObjectCollider_List = new List<Collider>();
    // 結合座標計算使う係数の大きさ
    private float frame = 1;

    // オブジェクト破壊時に呼び出すイベント登録
    public UnityEvent onBreakEvent;
    // オブジェクト結合時に呼び出すイベント登録
    public UnityEvent onConnectEvent;
    // オブジェクト破壊時にこのオブジェクトを登録しているオブジェクトのJointを外すためのイベント登録
    [Serializable] public class UnityEventPierce : UnityEvent<Collider> { }
    public UnityEventPierce onDisconnectEvent;

    // 結合する座標の設定
    private void DecideConnectPosition(Breaker breaker)
    {
        Vector3 moveDirection = breaker.GetMoveDirection();

        Vector3 movePosition = this.gameObject.transform.position + (-frame * moveDirection);

        this.gameObject.transform.position = movePosition;
    }

    private void OnDestroy()
    {
        Collider myCollider = this.gameObject.GetComponent<Collider>();
        DisconnectAll(myCollider);
        onDisconnectEvent?.Invoke(myCollider);
    }

    /// <summary>
    /// 刺突属性の呼び出し
    /// </summary>
    /// <param name="breaker">壊すものクラス</param>
    /// <returns>回復する耐久値、スコア</returns>
    public void CallPierce(Breaker breaker, Breakable breakable)
    {
        // コライダーの取得
        Collider breakerCollider = breaker.gameObject.GetComponent<Collider>();
        Collider myCollider = this.gameObject.GetComponent<Collider>();

        // 結合できないオブジェクトの場合
        if (!canConnect)
        {
            // 破壊時に呼び出されるイベントを呼び出す
            onBreakEvent?.Invoke();
            Destroy(this.gameObject);
            breakable.SetScore(0);
        }

        // 既に結合しているオブジェクトに対して、刺突属性で再び壊した場合
        if (isConnect)
        {
            DisconnectAll(myCollider);

            // 破壊時に呼び出されるイベントを呼び出す
            onBreakEvent?.Invoke();
            Destroy(this.gameObject);
            breakable.SetScore(0);
        }

        // 刺突結合の実行
        Connect(breaker, myCollider, breakerCollider);

        // 結合後の耐久値とスコアを設定する
        breakable.SetDurability(durabilityRecoveryAmount);
        breakable.SetScore(scoreRecoveryAmount);
    }

    /// <summary>
    /// 刺突属性による結合の開始
    /// </summary>
    /// <param name="breaker">壊すものクラス</param>
    /// <returns></returns>
    public void Connect(Breaker breaker, Collider myCollider, Collider breakerCollider)
    {
        // オブジェクトの動きの依存対象の設定
        // 既に結合しているオブジェクトの場合
        if (connectObjectCollider_List.Contains(breakerCollider))
        {
            return;
        }

        connectObjectCollider_List.Add(breakerCollider);

        // 結合したオブジェクト間の衝突判定の無効化
        Physics.IgnoreCollision(myCollider, breakerCollider, true);

        DecideConnectPosition(breaker);

        FixedJoint fixedJoint = this.gameObject.AddComponent<FixedJoint>();
        fixedJoint.connectedBody = breaker.GetRigidbody();

        isConnect = true;

        Pierce pierce = breakerCollider.GetComponent<Pierce>();
        pierce.onDisconnectEvent.AddListener(DisConnect);

        // 結合時に呼び出されるイベントを呼び出す
        onConnectEvent?.Invoke();  
    }

    /// <summary>
    /// 刺突属性による結合の解除
    /// </summary>
    /// <param name="breaker">壊すものクラス</param>
    /// <returns></returns>
    private void DisConnect(Collider breakerCollider)
    {
        // オブジェクトの動きの依存対象の解除
        FixedJoint[] fixedJoint_Array = this.gameObject.GetComponents<FixedJoint>();
        Rigidbody rigidbody = breakerCollider.GetComponent<Rigidbody>();

        // FixedJointを全てから解除対象のみ選択
        foreach (FixedJoint fixedJoint in fixedJoint_Array)
        {
            if (fixedJoint.connectedBody == rigidbody)
            {
                Destroy(fixedJoint);
            }
        }

        connectObjectCollider_List.Remove(breakerCollider);

        if (connectObjectCollider_List.Count <= 0) isConnect = false;
        // 結合したオブジェクト間の衝突判定の有効化
        Physics.IgnoreCollision(this.gameObject.GetComponent<Collider>(), breakerCollider, false);
    }

    /// <summary>
    /// 刺突属性による結合の解除
    /// </summary>
    /// <param name="breaker">壊すものクラス</param>
    /// <returns></returns>
    private void DisconnectAll(Collider myCollider)
    {
        if (connectObjectCollider_List.Count <= 0) return;

        // FixedJointを全て取得し、
        FixedJoint[] fixedJoint_List = this.gameObject.GetComponents<FixedJoint>();

        // オブジェクトの動きの依存対象の解除
        foreach (FixedJoint fixedJoint in fixedJoint_List)
        {
            Destroy(fixedJoint);
        }

        isConnect = false;
        // 結合したオブジェクト間の衝突判定の有効化
        foreach (Collider connectObjectCollider in connectObjectCollider_List)
        {
            Physics.IgnoreCollision(myCollider, connectObjectCollider, false);
        }

        connectObjectCollider_List.Clear();
    }
}