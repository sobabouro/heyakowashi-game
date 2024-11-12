using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MixedReality.Toolkit.SpatialManipulation;
using UnityEngine.Events;

public class Pierce : MonoBehaviour
{
    [SerializeField, Tooltip("�񕜑ϋv�l")]
    private int durabilityRecoveryAmount;
    [SerializeField, Tooltip("�񕜃X�R�A")]
    private int scoreRecoveryAmount;
    [SerializeField, Tooltip("�����\�H")]
    private bool canConnect;
    // �������Ă���H
    private bool isPierce = false;
    // �������W�v�Z�g���W���̑傫��
    private float frame = 2;

    private static Dictionary<Collider, List<Collider>> piereceConnectDictionary = new Dictionary<Collider, List<Collider>>();

    // �I�u�W�F�N�g�j�󎞂ɌĂяo���C�x���g�o�^
    public UnityEvent onBreakEvent;
    // �I�u�W�F�N�g�������ɌĂяo���C�x���g�o�^
    public UnityEvent onConnectEvent;

    // ����������W�̐ݒ�
    private void DecideConnectPosition(Breaker breaker)
    {
        Vector3 moveDirection = breaker.GetMoveDirection();

        Vector3 movePosition = this.gameObject.transform.position + (-frame * moveDirection);

        this.gameObject.transform.position = movePosition;
    }

    private void OnDestroy()
    {
        Collider myCollider = this.gameObject.GetComponent<Collider>();
        DisconnectAll(myCollider);
        RemoveAllFixedJoint(myCollider);
    }

    /// <summary>
    /// �h�ˑ����̌Ăяo��
    /// </summary>
    /// <param name="breaker">�󂷂��̃N���X�A�󂳂����̃N���X</param>
    /// <returns>�񕜂���ϋv�l�A�X�R�A</returns>
    public void CallPierce(Breaker breaker, Breakable breakable)
    {
        // �R���C�_�[�̎擾
        Collider breakerCollider = breaker.gameObject.GetComponent<Collider>();
        Collider myCollider = this.gameObject.GetComponent<Collider>();

        // �����ł��Ȃ��I�u�W�F�N�g�̏ꍇ
        if (!canConnect)
        {
            // �j�󎞂ɌĂяo�����C�x���g���Ăяo��
            onBreakEvent?.Invoke();
            breakable.SetScore(0);
            Destroy(this.gameObject);
            return;
        }

        // ���Ɍ������Ă���I�u�W�F�N�g�ɑ΂��āA�h�ˑ����ōĂщ󂵂��ꍇ
        if (isPierce)
        {
            DisconnectAll(myCollider);

            // �j�󎞂ɌĂяo�����C�x���g���Ăяo��
            onBreakEvent?.Invoke();
            breakable.SetScore(0);
            Destroy(this.gameObject);
            return;
        }

        // �h�ˌ����̎��s
        Connect(breaker, myCollider, breakerCollider);

        // ������̑ϋv�l�ƃX�R�A��ݒ肷��
        breakable.SetDurability(durabilityRecoveryAmount);
        breakable.SetScore(scoreRecoveryAmount);
    }

    /// <summary>
    /// �h�ˑ����ɂ�錋���̊J�n
    /// </summary>
    /// <param name="breaker">�󂷂��̃N���X�A�󂷑��R���C�_�[�A�󂳂�鑤�R���C�_�[</param>
    /// <returns></returns>
    public void Connect(Breaker breaker, Collider myCollider, Collider breakerCollider)
    {
        List<Collider> connectObjectCollider_List = new List<Collider>();
        connectObjectCollider_List = piereceConnectDictionary[myCollider];

        // �I�u�W�F�N�g�̓����̈ˑ��Ώۂ̐ݒ�
        // ���Ɍ������Ă���I�u�W�F�N�g�̏ꍇ
        if (connectObjectCollider_List.Contains(breakerCollider)) return;

        // �h�ˌ����S�̂̃��X�g�̍X�V
        DictionaryAdd(myCollider, connectObjectCollider_List);

        // ���������I�u�W�F�N�g�Ԃ̏Փ˔���̖�����
        Physics.IgnoreCollision(myCollider, breakerCollider, true);

        DecideConnectPosition(breaker);

        // �󂳂�鑤�ւ̐ݒ�
        FixedJoint fixedJoint = this.gameObject.AddComponent<FixedJoint>();
        fixedJoint.connectedBody = breaker.GetRigidbody();
        // �󂷑��ւ̐ݒ�
        Rigidbody rigidbody = myCollider.GetComponent<Rigidbody>();
        fixedJoint = breaker.gameObject.AddComponent<FixedJoint>();
        fixedJoint.connectedBody = rigidbody;

        isPierce = true;

        // �������ɌĂяo�����C�x���g���Ăяo��
        onConnectEvent?.Invoke();  
    }

    /// <summary>
    /// �h�ˑ����ɂ�錋���̉���
    /// </summary>
    /// <param name="breaker">�󂷂��̃N���X�A�󂷑��R���C�_�[�A�󂳂�鑤�R���C�_�[</param>
    /// <returns></returns>
    private void DisConnect(Collider myCollider, Collider breakerCollider)
    {
        // FixedJoint�������ΏۊԂ̂݉���
        Rigidbody rigidbody = breakerCollider.GetComponent<Rigidbody>();
        RemoveTargetFixedJoint(myCollider, rigidbody);
        rigidbody = myCollider.GetComponent<Rigidbody>();
        RemoveTargetFixedJoint(breakerCollider, rigidbody);

        piereceConnectDictionary[myCollider].Remove(breakerCollider);
        piereceConnectDictionary[breakerCollider].Remove(myCollider);

        if (piereceConnectDictionary[myCollider].Count <= 0) isPierce = false;
        // ���������I�u�W�F�N�g�Ԃ̏Փ˔���̗L����
        Physics.IgnoreCollision(myCollider, breakerCollider, false);
    }

    /// <summary>
    /// �h�ˑ����ɂ�錋���̉���
    /// </summary>
    /// <param name="breaker">�R���C�_�[</param>
    /// <returns></returns>
    private void DisconnectAll(Collider targetCollider)
    {
        if (piereceConnectDictionary[targetCollider].Count <= 0) return;

        // FixedJoint��S�Ď擾���A
        FixedJoint[] fixedJoint_List = this.gameObject.GetComponents<FixedJoint>();
        // �I�u�W�F�N�g�̓����̈ˑ��Ώۂ̉���
        foreach (FixedJoint fixedJoint in fixedJoint_List)
        {
            Destroy(fixedJoint);
        }

        isPierce = false;
        
        foreach (Collider connectObjectCollider in piereceConnectDictionary[targetCollider])
        {
            // ���������I�u�W�F�N�g�Ԃ̏Փ˔���̗L����
            Physics.IgnoreCollision(targetCollider, connectObjectCollider, false);
        }

        DictionaryRemove(targetCollider);
    }

    // 
    private void DictionaryAdd(Collider targetCollider, List<Collider> connectObjectCollider_List)
    {
        // �^�[�Q�b�g���L�[�Ƃ��������̓o�^
        piereceConnectDictionary.Add(targetCollider, connectObjectCollider_List);

        // 
        foreach(Collider collider in connectObjectCollider_List)
        {
            List<Collider> list = piereceConnectDictionary[collider];
            if (list.Contains(targetCollider)) list.Add(targetCollider);
            piereceConnectDictionary[collider] = list;
        }
    }

    private void DictionaryRemove(Collider targetCollider)
    {
        Rigidbody rigidbody = targetCollider.GetComponent<Rigidbody>();
        foreach (KeyValuePair<Collider, List<Collider>> pair in piereceConnectDictionary)
        {
            if (pair.Key == targetCollider)
            {
                piereceConnectDictionary.Remove(targetCollider);
                continue;
            }
            else
            {
                List<Collider> list = pair.Value;
                list.Remove(targetCollider);
                piereceConnectDictionary[pair.Key] = list;
            }
            
        }
    }

    private void RemoveTargetFixedJoint(Collider targetCollider, Rigidbody removeRigidbody)
    {
        // �I�u�W�F�N�g�̓����̈ˑ��Ώۂ̉���
        FixedJoint[] fixedJoint_Array = targetCollider.GetComponents<FixedJoint>();

        // FixedJoint��S�Ă�������Ώۂ̂ݑI��
        foreach (FixedJoint fixedJoint in fixedJoint_Array)
        {
            if (fixedJoint.connectedBody == removeRigidbody)
            {
                Destroy(fixedJoint);
            }
        }
    }

    private void RemoveAllFixedJoint(Collider targetCollider)
    {
        // �I�u�W�F�N�g�̓����̈ˑ��Ώۂ̉���
        FixedJoint[] fixedJoint_Array = targetCollider.GetComponents<FixedJoint>();
        Rigidbody rigidbody = targetCollider.GetComponent<Rigidbody>();

        // FixedJoint��S�Ă�������Ώۂ̂ݑI��
        foreach (FixedJoint fixedJoint in fixedJoint_Array)
        {
            Destroy(fixedJoint);
        }

        foreach (Collider collider in piereceConnectDictionary[targetCollider])
        {
            RemoveTargetFixedJoint(collider, rigidbody);
        }
    }
}