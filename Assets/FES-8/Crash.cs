using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Crash : MonoBehaviour
{
    // 破壊後のオブジェクトを呼び出すかのフラグ
    [SerializeField]
    private bool _canCallBrokenObject;
    // オブジェクトの破壊後に呼び出されるオブジェクト
    [SerializeField]
    private GameObject _brokenObjectPrefab;
    // オブジェクト破壊時に呼び出すイベント登録
    public UnityEvent onBreakEvent;
    // 破壊後のオブジェクトを呼び出す際に加える外向きの力
    private Vector3 _addImpulse;

    // Start is called before the first frame update
    void Start()
    {

    }

    // 壊属性によるオブジェクトの破壊処理が呼び出される際に呼び出す
    public void CallCrash()
    {
        /* // 自身の当たり判定を消失させる
        ここに処理を記述
         */

        // 破壊時に呼び出されるイベントを呼び出す
        onBreakEvent?.Invoke();

        // フラグによって破壊後のオブジェクトを呼び出したりする
        if (_canCallBrokenObject)
        {
            CallBrokenObject();
        }

        // オブジェクトを破壊する
        Destroy(this.gameObject);
    }

    // 破壊後にオブジェクトを作る際に呼び出す
    private void CallBrokenObject()
    {
        /* // 破壊後に呼び出すオブジェクトを中心から外側に向けてある程度の力(_addForce)を入れてオブジェクトを生成する
        ここに処理を記述
         */
    }

}
