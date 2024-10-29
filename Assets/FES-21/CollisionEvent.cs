using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollisionEvent : MonoBehaviour
{
    [Serializable] public class UnityEventArgCollision : UnityEvent<Collision> { }

    public static bool canEventCall;

    [SerializeField] public UnityEventArgCollision collisionEvnetEnter;
    [SerializeField] public UnityEventArgCollision collisionEvnetStay;
    [SerializeField] public UnityEventArgCollision collisionEvnetExit;

    private void OnCollisionEnter(Collision collision)
    {
        if (!canEventCall) return;
        collisionEvnetEnter.Invoke(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!canEventCall) return;
        collisionEvnetEnter.Invoke(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!canEventCall) return;
        collisionEvnetEnter.Invoke(collision);
    }
}
