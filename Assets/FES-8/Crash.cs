using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Crash : MonoBehaviour
{
    [SerializeField, Tooltip("破壊後のオブジェクトを生成する？")]
    private bool _canCreateBrokenObject;
    [SerializeField, Tooltip("破壊後のオブジェクト")]
    private GameObject _brokenObjectPrefab;
    [SerializeField, Tooltip("破壊後のオブジェクトを生成した際に加える力")]
    private float _addImpulse = 1;

    // オブジェクト破壊時に呼び出すイベント登録
    public UnityEvent onBreakEvent;

    // Start is called before the first frame update
    void Start()
    {
        onBreakEvent.AddListener(DebugMessage);
    }

    /// <summary>
    /// 壊属性によるオブジェクトの破壊時に呼び出す
    /// </summary>
    public void CallCrash()
    {
        // 自身の当たり判定を消失させる
        this.gameObject.GetComponent<Collider>().enabled = false;

        // 破壊時に呼び出されるイベントを呼び出す
        onBreakEvent?.Invoke();

        // フラグによって破壊後のオブジェクトを呼び出したりする
        if (_canCreateBrokenObject)
        {
            CreateBrokenObject();
        }

        // オブジェクトを破壊する
        Debug.Log("CrashDestroy! : " + this.gameObject);
        Destroy(this.gameObject);
    }

    /// <summary>
    /// 破壊後にオブジェクトを作る際に呼び出す
    /// </summary>
    private void CreateBrokenObject()
    {
        Debug.Log("CallBrokenObject!");
        // 破壊後に呼び出すオブジェクトを生成して、外側に向けてある程度の力(_addForce)を入れてオブジェクトを動かす
        Transform parentTransform = _brokenObjectPrefab.transform;

        Debug.Log("ParentTagCheck : " + _brokenObjectPrefab.tag);

        if (_brokenObjectPrefab.CompareTag("BreakableObject"))
        {
            GameObject createObject = Instantiate(_brokenObjectPrefab, this.gameObject.transform.position + parentTransform.localPosition, this.gameObject.transform.rotation * parentTransform.localRotation);

            Rigidbody rigidbody = createObject.GetComponent<Rigidbody>();

            rigidbody.AddForce(_addImpulse * Vector3.Normalize(parentTransform.localPosition), ForceMode.Impulse);
        }
        else
        {
            // 子オブジェクトを全て取得する(一つ下の階層を対象としてBreakableObjectのタグが付いたモノを呼び出す)
            foreach (Transform child in parentTransform)
            {
                Debug.Log("ChildTagCheck : " + child.tag);
                if (child.CompareTag("BreakableObject") == false) continue;
                GameObject createObject = Instantiate(child.gameObject, this.gameObject.transform.position + child.localPosition, this.gameObject.transform.rotation * child.localRotation);

                Rigidbody rigidbody = createObject.GetComponent<Rigidbody>();

                rigidbody.AddForce(_addImpulse * Vector3.Normalize(child.localPosition), ForceMode.Impulse);
            }
        }
    }

    private void DebugMessage()
    {
        Debug.Log("onBreakEvent.Invoke!");
    }

}
