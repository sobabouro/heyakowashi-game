using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnCollisionEnterTest : MonoBehaviour
{

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject + "�ɓ������Ă���I");
    }
}