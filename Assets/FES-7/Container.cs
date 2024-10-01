using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{
    [SerializeField]
    private GameObject mainObject;
    [SerializeField]
    private GameObject registeredObject;

    private void Start()
    {
        SetRegisteredObject(mainObject);
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
        CollisionEvent collisionEvent = this.gameObject.GetComponent<CollisionEvent>();
        if(targetObject != mainObject) collisionEvent.collisionEvnetEnter.RemoveListener(registeredObject.GetComponent<Breaker>().Attack);
        registeredObject = targetObject;
        collisionEvent.collisionEvnetEnter.AddListener(registeredObject.GetComponent<Breaker>().Attack);
        registeredObject.GetComponent<Breaker>().SetRigidbody(this.gameObject.GetComponent<Rigidbody>());
    }
    public GameObject GetRegisteredObject()
    {
        return registeredObject;
    }
}
