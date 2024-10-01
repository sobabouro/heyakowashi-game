using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{
    [SerializeField]
    private GameObject mainObject;
    [SerializeField]
    private GameObject registeredObject;

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
        collisionEvent.collisionEvnetEnter.RemoveListener(registeredObject.GetComponent<Breaker>().Attack);
        registeredObject = targetObject;
        collisionEvent.collisionEvnetEnter.AddListener(registeredObject.GetComponent<Breaker>().Attack);
    }
    public GameObject GetRegisteredObject()
    {
        return registeredObject;
    }
}
