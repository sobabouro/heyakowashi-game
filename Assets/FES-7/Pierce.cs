using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MixedReality.Toolkit.SpatialManipulation;

public class Pierce : MonoBehaviour
{
    [SerializeField]
    private int durabilityRecoveryAmount;

    // �h�ˑ����ɂ�錋���̊J�n
    public int Connect(Container container)
    {
        // �R���e�i�̎q�I�u�W�F�N�g�ɂ����rigidbody�̔j��
        Rigidbody rigidbody = this.gameObject.GetComponent<Rigidbody>();
        Destroy(rigidbody);

        // ���g�̐e��Breaker.container�ɂ���
<<<<<<< HEAD
        this.gameObject.transform.SetParent(breaker.GetContainer().gameObject.transform);

        // Container�N���X�̓o�^�I�u�W�F�N�g�����g�ɂ���
        GameObject container = breaker.GetContainer().gameObject;
        container.GetComponent<Container>().SetRegisteredObject(this.gameObject);   

        // �񕜂���ϋv�l��Ԃ�
        return durabilityRecoveryAmount; 
=======
        this.gameObject.transform.SetParent(container.gameObject.transform);

        // Container�N���X�̓o�^�I�u�W�F�N�g�����g�ɂ���
        container.SetRegisteredObject(this.gameObject);

        // �񕜂���ϋv�l��Ԃ�
        return durabilityRecoveryAmount;
>>>>>>> FES-7-突属性によるオブジェクトの破壊処理
    }

    // ����������W�̐ݒ�
    private void DecideConnectPosition()
    {

    }
}