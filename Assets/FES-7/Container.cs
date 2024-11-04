using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MixedReality.Toolkit.SpatialManipulation;

public class Container : MonoBehaviour
{
    [SerializeField]
    private GameObject mainObject;
    [SerializeField]
    private GameObject registeredObject;
    private CollisionEvent collisionEvent;
    private new Rigidbody rigidbody;

    private void Start()
    {
        collisionEvent = this.gameObject.GetComponent<CollisionEvent>();
        rigidbody = this.gameObject.GetComponent<Rigidbody>();
        SetMainRegister();
    }

    public void SetMainObject(GameObject targetObject)
    {
        mainObject = targetObject;
    }

    public GameObject GetMainObject()
    {
        return mainObject;
    }

    public void SetRegisteredObject(GameObject targetObject)
    {
        // コンテナの子オブジェクトにされるrigidbodyの破棄
        if (targetObject != registeredObject) collisionEvent.collisionEvnetEnter.RemoveAllListeners();
        registeredObject = targetObject;
        collisionEvent.collisionEvnetEnter.AddListener(registeredObject.GetComponent<Breaker>().Attack);

        // Breakerクラスに保存されるrigidbodyに登録
        registeredObject.GetComponent<Breaker>().SetRigidbody(rigidbody);

        // HoloLens2での操作での座標移動の対象をcontainerにする
        HostTransformSwitch(registeredObject);
    }
    public GameObject GetRegisteredObject()
    {
        return registeredObject;
    }

    public void SetMainRegister()
    {
        if (mainObject == registeredObject) return;
        CollisionEvent collisionEvent = this.gameObject.GetComponent<CollisionEvent>();
        registeredObject = mainObject;
        collisionEvent.collisionEvnetEnter.AddListener(registeredObject.GetComponent<Breaker>().Attack);

        // Breakerクラスに保存されるrigidbodyに登録
        registeredObject.GetComponent<Breaker>().SetRigidbody(rigidbody);

        // HoloLens2での操作での座標移動の対象をcontainerにする
        HostTransformSwitch(mainObject);
    }

    // 引数ObjectのHoloLens2での操作での座標移動の対象をcontainerにする
    private void HostTransformSwitch(GameObject targetObject)
    {
        if (targetObject.GetComponent<ObjectManipulator>() == null) return;
        targetObject.GetComponent<ObjectManipulator>().HostTransform = this.gameObject.transform;
    }
}
