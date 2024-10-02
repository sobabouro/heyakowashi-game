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
    private Rigidbody rigidbody;

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
        if (targetObject != registeredObject) collisionEvent.collisionEvnetEnter.RemoveListener(registeredObject.GetComponent<Breaker>().Attack);
        registeredObject = targetObject;
        collisionEvent.collisionEvnetEnter.AddListener(registeredObject.GetComponent<Breaker>().Attack);

        // Breaker�N���X�ɕۑ������rigidbody�ɓo�^
        registeredObject.GetComponent<Breaker>().SetRigidbody(rigidbody);

        // HoloLens2�ł̑���ł̍��W�ړ��̑Ώۂ�container�ɂ���
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

        // Breaker�N���X�ɕۑ������rigidbody�ɓo�^
        registeredObject.GetComponent<Breaker>().SetRigidbody(this.gameObject.GetComponent<Rigidbody>());

        // HoloLens2�ł̑���ł̍��W�ړ��̑Ώۂ�container�ɂ���
        HostTransformSwitch(mainObject);
    }

    // ����Object��HoloLens2�ł̑���ł̍��W�ړ��̑Ώۂ�container�ɂ���
    private void HostTransformSwitch(GameObject targetObject)
    {
        if (targetObject.GetComponent<ObjectManipulator>() == null) return;
        targetObject.GetComponent<ObjectManipulator>().HostTransform = this.gameObject.transform;
    }
}
