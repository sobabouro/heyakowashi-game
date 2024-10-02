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
        // �R���e�i�̎q�I�u�W�F�N�g�ɂ����rigidbody�̔j��
        Rigidbody rigidbody = this.gameObject.GetComponent<Rigidbody>();
        Destroy(rigidbody);

        // ���g�̐e��Breaker.container�ɂ���
        this.gameObject.transform.SetParent(breaker.GetContainer());

        // Container�N���X�̓o�^�I�u�W�F�N�g�����g�ɂ���
        GameObject container = breaker.GetContainer().gameObject;
        container.GetComponent<Container>().SetRegisteredObject(this.gameObject);   

        breaker.enabled = false;

        // �񕜂���ϋv�l��Ԃ�
        return durabilityRecoveryAmount; 
    }

    // ����������W�̐ݒ�
    private void DecideConnectPosition()
    {
        
    }
}
