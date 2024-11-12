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
    private bool isPierce = false;
    // 結合座標計算使う係数の大きさ
    private float frame = 2;

    private static Dictionary<Collider, List<Collider>> piereceConnectDictionary = new Dictionary<Collider, List<Collider>>();

    // オブジェクト破壊時に呼び出すイベント登録
    public UnityEvent onBreakEvent;
    // オブジェクト結合時に呼び出すイベント登録
    public UnityEvent onConnectEvent;

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
        RemoveAllFixedJoint(myCollider);
    }

    /// <summary>
    /// 刺突属性の呼び出し
    /// </summary>
    /// <param name="breaker">壊すものクラス、壊されるものクラス</param>
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
            breakable.SetScore(0);
            Destroy(this.gameObject);
            return;
        }

        // 既に結合しているオブジェクトに対して、刺突属性で再び壊した場合
        if (isPierce)
        {
            DisconnectAll(myCollider);

            // 破壊時に呼び出されるイベントを呼び出す
            onBreakEvent?.Invoke();
            breakable.SetScore(0);
            Destroy(this.gameObject);
            return;
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
    /// <param name="breaker">壊すものクラス、壊す側コライダー、壊される側コライダー</param>
    /// <returns></returns>
    public void Connect(Breaker breaker, Collider myCollider, Collider breakerCollider)
    {
        List<Collider> connectObjectCollider_List = new List<Collider>();
        connectObjectCollider_List = piereceConnectDictionary[myCollider];

        // オブジェクトの動きの依存対象の設定
        // 既に結合しているオブジェクトの場合
        if (connectObjectCollider_List.Contains(breakerCollider)) return;

        // 刺突結合全体のリストの更新
        DictionaryAdd(myCollider, connectObjectCollider_List);

        // 結合したオブジェクト間の衝突判定の無効化
        Physics.IgnoreCollision(myCollider, breakerCollider, true);

        DecideConnectPosition(breaker);

        // 壊される側への設定
        FixedJoint fixedJoint = this.gameObject.AddComponent<FixedJoint>();
        fixedJoint.connectedBody = breaker.GetRigidbody();
        // 壊す側への設定
        Rigidbody rigidbody = myCollider.GetComponent<Rigidbody>();
        fixedJoint = breaker.gameObject.AddComponent<FixedJoint>();
        fixedJoint.connectedBody = rigidbody;

        isPierce = true;

        // 結合時に呼び出されるイベントを呼び出す
        onConnectEvent?.Invoke();  
    }

    /// <summary>
    /// 刺突属性による結合の解除
    /// </summary>
    /// <param name="breaker">壊すものクラス、壊す側コライダー、壊される側コライダー</param>
    /// <returns></returns>
    private void DisConnect(Collider myCollider, Collider breakerCollider)
    {
        // FixedJointを解除対象間のみ解除
        Rigidbody rigidbody = breakerCollider.GetComponent<Rigidbody>();
        RemoveTargetFixedJoint(myCollider, rigidbody);
        rigidbody = myCollider.GetComponent<Rigidbody>();
        RemoveTargetFixedJoint(breakerCollider, rigidbody);

        piereceConnectDictionary[myCollider].Remove(breakerCollider);
        piereceConnectDictionary[breakerCollider].Remove(myCollider);

        if (piereceConnectDictionary[myCollider].Count <= 0) isPierce = false;
        // 結合したオブジェクト間の衝突判定の有効化
        Physics.IgnoreCollision(myCollider, breakerCollider, false);
    }

    /// <summary>
    /// 刺突属性による結合の解除
    /// </summary>
    /// <param name="breaker">コライダー</param>
    /// <returns></returns>
    private void DisconnectAll(Collider targetCollider)
    {
        if (piereceConnectDictionary[targetCollider].Count <= 0) return;

        // FixedJointを全て取得し、
        FixedJoint[] fixedJoint_List = this.gameObject.GetComponents<FixedJoint>();
        // オブジェクトの動きの依存対象の解除
        foreach (FixedJoint fixedJoint in fixedJoint_List)
        {
            Destroy(fixedJoint);
        }

        isPierce = false;
        
        foreach (Collider connectObjectCollider in piereceConnectDictionary[targetCollider])
        {
            // 結合したオブジェクト間の衝突判定の有効化
            Physics.IgnoreCollision(targetCollider, connectObjectCollider, false);
        }

        DictionaryRemove(targetCollider);
    }

    // 
    private void DictionaryAdd(Collider targetCollider, List<Collider> connectObjectCollider_List)
    {
        // ターゲットをキーとした辞書の登録
        piereceConnectDictionary.Add(targetCollider, connectObjectCollider_List);

        // 
        foreach(Collider collider in connectObjectCollider_List)
        {
            List<Collider> list = piereceConnectDictionary[collider];
            if (list.Contains(targetCollider)) list.Add(targetCollider);
            piereceConnectDictionary[collider] = list;
        }
    }

    private void DictionaryRemove(Collider targetCollider)
    {
        Rigidbody rigidbody = targetCollider.GetComponent<Rigidbody>();
        foreach (KeyValuePair<Collider, List<Collider>> pair in piereceConnectDictionary)
        {
            if (pair.Key == targetCollider)
            {
                piereceConnectDictionary.Remove(targetCollider);
                continue;
            }
            else
            {
                List<Collider> list = pair.Value;
                list.Remove(targetCollider);
                piereceConnectDictionary[pair.Key] = list;
            }
            
        }
    }

    private void RemoveTargetFixedJoint(Collider targetCollider, Rigidbody removeRigidbody)
    {
        // オブジェクトの動きの依存対象の解除
        FixedJoint[] fixedJoint_Array = targetCollider.GetComponents<FixedJoint>();

        // FixedJointを全てから解除対象のみ選択
        foreach (FixedJoint fixedJoint in fixedJoint_Array)
        {
            if (fixedJoint.connectedBody == removeRigidbody)
            {
                Destroy(fixedJoint);
            }
        }
    }

    private void RemoveAllFixedJoint(Collider targetCollider)
    {
        // オブジェクトの動きの依存対象の解除
        FixedJoint[] fixedJoint_Array = targetCollider.GetComponents<FixedJoint>();
        Rigidbody rigidbody = targetCollider.GetComponent<Rigidbody>();

        // FixedJointを全てから解除対象のみ選択
        foreach (FixedJoint fixedJoint in fixedJoint_Array)
        {
            Destroy(fixedJoint);
        }

        foreach (Collider collider in piereceConnectDictionary[targetCollider])
        {
            RemoveTargetFixedJoint(collider, rigidbody);
        }
    }
}