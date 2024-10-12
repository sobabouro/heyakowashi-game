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
    private List<GameObject> _brokenObjectPrefabList;
    // オブジェクト破壊時に呼び出すイベント登録
    public UnityEvent onBreakEvent;
    // 破壊後のオブジェクトを呼び出す際に加える外向きの力
    [SerializeField]
    private float _addImpulse = 1;

    // Start is called before the first frame update
    void Start()
    {
        onBreakEvent.AddListener(DebugMessage);
    }

    // 壊属性によるオブジェクトの破壊処理が呼び出される際に呼び出す
    public void CallCrash()
    {
        // 自身の当たり判定を消失させる
        this.gameObject.GetComponent<Collider>().enabled = false;


        // 破壊時に呼び出されるイベントを呼び出す
        onBreakEvent?.Invoke();

        // フラグによって破壊後のオブジェクトを呼び出したりする
        if (_canCallBrokenObject)
        {
            CallBrokenObject();
        }

        // オブジェクトを破壊する
        Debug.Log("CrashDestroy! : " + this.gameObject);
        Destroy(this.gameObject);
    }

    // 破壊後にオブジェクトを作る際に呼び出す
    private void CallBrokenObject()
    {
        Debug.Log("CallBrokenObject!");
        // 破壊後に呼び出すオブジェクトを生成して、外側に向けてある程度の力(_addForce)を入れてオブジェクトを動かす
        foreach (GameObject targetObject in _brokenObjectPrefabList)
        {
            GameObject createObject = Instantiate(targetObject, this.gameObject.transform.position, this.gameObject.transform.rotation);

            Rigidbody rigidbody = createObject.GetComponent<Rigidbody>();
            Vector3 insideUnitSphere = Random.insideUnitSphere; // 半径 1 の球体の内部のランダムな点を返します

            rigidbody.AddForce(_addImpulse * insideUnitSphere, ForceMode.Impulse);
        }
    }

    private void DebugMessage()
    {
        Debug.Log("onBreakEvent.Invoke!");
    }

}
