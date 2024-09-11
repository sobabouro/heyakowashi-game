using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollisionEvent : MonoBehaviour
{
    [Serializable] public class UnityEventArgObj : UnityEvent<GameObject> { }

    [SerializeField] UnityEventArgObj collisionEvnetEnter;
    [SerializeField] UnityEventArgObj collisionEvnetStay;
    [SerializeField] UnityEventArgObj collisionEvnetExit;

    private void OnCollisionEnter(Collision collision)
    {
        collisionEvnetEnter.Invoke(collision.gameObject);
    }

    private void OnCollisionStay(Collision collision)
    {
        collisionEvnetEnter.Invoke(collision.gameObject);
    }

    private void OnCollisionExit(Collision collision)
    {
        collisionEvnetEnter.Invoke(collision.gameObject);
    }
}
