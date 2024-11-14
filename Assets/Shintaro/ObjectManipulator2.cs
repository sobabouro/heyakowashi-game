using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManipulator2 : MonoBehaviour
{
    [Header("追従するオブジェクト")]
    [SerializeField] private Transform SynchronizedObjectTransform = null;

    private Transform m_transform;

    private GameObject nearObject = null;

    private void Start()
    {
        m_transform = gameObject.transform;
    }
    private void Update()
    {
        SynchronizeMovemen();
    }

    private void SynchronizeMovemen()
    {
        if (SynchronizedObjectTransform == null) return;
        SynchronizedObjectTransform.transform.position = m_transform.position;
        SynchronizedObjectTransform.transform.rotation = m_transform.rotation;
    }

    public void HoldSynchronizedObject()
    {
        if (nearObject == null) return;
        SynchronizedObjectTransform = nearObject.transform;
    }

    public void ReleaseSynchronizedObject()
    {
        SynchronizedObjectTransform = null;
    }

    private void OnTriggerEnter(Collider collider)
    {
        nearObject = collider.gameObject;
        gameObject.GetComponent<Renderer>().material.color = Color.green;
    }

    private void OnTriggerExit(Collider collider)
    {
        if(nearObject == collider.gameObject)
        {
            nearObject = null;
            gameObject.GetComponent<Renderer>().material.color = Color.white;
        }
    }
}
