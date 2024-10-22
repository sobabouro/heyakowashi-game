using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollisionEvent : MonoBehaviour
{
    [Serializable] public class UnityEventArgCollision : UnityEvent<Collision> { }

    [SerializeField] public UnityEventArgCollision collisionEvnetEnter;
    [SerializeField] public UnityEventArgCollision collisionEvnetStay;
    [SerializeField] public UnityEventArgCollision collisionEvnetExit;

    private void OnCollisionEnter(Collision collision)
    {
        collisionEvnetEnter.Invoke(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        collisionEvnetEnter.Invoke(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        collisionEvnetEnter.Invoke(collision);
    }
}
