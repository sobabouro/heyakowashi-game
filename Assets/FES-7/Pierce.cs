using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MixedReality.Toolkit.SpatialManipulation;

public class Pierce : MonoBehaviour
{
    [SerializeField]
    private int durabilityRecoveryAmount;

    // �h�ˑ����ɂ�錋���̊J�n
    public int Connect(Breaker breaker)
    {
        this.gameObject.transform.SetParent(breaker.GetContainer());        // ���g�̐e��Breaker.container�ɂ���
        GameObject container = breaker.GetContainer().gameObject;
        container.GetComponent<Container>().SetRegisteredObject(this.gameObject);   // Container�N���X�̓o�^�I�u�W�F�N�g�����g�ɂ���
        breaker.enabled = false;
        this.gameObject.GetComponent<ObjectManipulator>().HostTransform = container.transform; // HoloLens2�ł̑���ł̍��W�ړ��̑Ώۂ�container�ɂ���

        return durabilityRecoveryAmount; // �񕜂���ϋv�l��Ԃ�
    }

    // ����������W�̐ݒ�
    private void DecideConnectPosition()
    {
        
    }
}
