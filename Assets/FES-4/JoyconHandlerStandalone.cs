using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoyconHandlerStandalone : MonoBehaviour
{
    private Transform m_transform;

    // Values made available via Unity
    public float[] stick;
    public Quaternion orientation;

    private void Start()
    {
        m_transform = gameObject.transform;
    }

    public void UpdateData(UDPServer.Message message)
    {
        m_transform.rotation = message.ToQuaternion();
    }
}
