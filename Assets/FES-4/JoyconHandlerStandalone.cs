using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoyconHandlerStandalone : MonoBehaviour
{
    [SerializeField]
    private Transform cameraTransform;

    // Values made available via Unity
    public float[] stick;
    public Quaternion orientation;

    public void UpdateData(UDPServer.Message message)
    {
        orientation = cameraTransform.rotation * message.ToQuaternion();
    }

    // Update is called once per frame
    void Update()
    {
        // ‰ñ“]
        gameObject.transform.rotation = orientation;
    }
}
