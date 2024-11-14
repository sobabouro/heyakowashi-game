using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MixedReality.Toolkit.SpatialManipulation;
using UnityEngine.Events;
using System;

public class Pierce : MonoBehaviour
{
    [SerializeField, Tooltip("�񕜑ϋv�l")]
    private int durabilityRecoveryAmount;
    [SerializeField, Tooltip("�񕜃X�R�A")]
    private int scoreRecoveryAmount;
    [SerializeField, Tooltip("�����\�H")]
    private bool canConnect;
    // �������Ă���H
    private bool isConnect = false;
    // �h�ˌ������Ă��鎞�̌�������̃I�u�W�F�N�g
    private List<Collider> connectObjectCollider_List = new List<Collider>();
    // �������W�v�Z�g���W���̑傫��
    private float frame = 2;

    // �I�u�W�F�N�g�j�󎞂ɌĂяo���C�x���g�o�^
    public UnityEvent onBreakEvent;
    // �I�u�W�F�N�g�������ɌĂяo���C�x���g�o�^
    public UnityEvent onConnectEvent;
    // �I�u�W�F�N�g�j�󎞂ɂ��̃I�u�W�F�N�g��o�^���Ă���I�u�W�F�N�g��Joint���O�����߂̃C�x���g�o�^
    [Serializable] public class UnityEventPierce : UnityEvent<Collider> { }
    public UnityEventPierce onDisconnectEvent;

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
        onDisconnectEvent?.Invoke(myCollider);
    }

    /// <summary>
    /// �h�ˑ����̌Ăяo��
    /// </summary>
    /// <param name="breaker">�󂷂��̃N���X</param>
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
            Destroy(this.gameObject);
            breakable.SetScore(0);
        }

        // ���Ɍ������Ă���I�u�W�F�N�g�ɑ΂��āA�h�ˑ����ōĂщ󂵂��ꍇ
        if (isConnect)
        {
            DisconnectAll(myCollider);

            // �j�󎞂ɌĂяo�����C�x���g���Ăяo��
            onBreakEvent?.Invoke();
            Destroy(this.gameObject);
            breakable.SetScore(0);
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
    /// <param name="breaker">�󂷂��̃N���X</param>
    /// <returns></returns>
    public void Connect(Breaker breaker, Collider myCollider, Collider breakerCollider)
    {
        // �I�u�W�F�N�g�̓����̈ˑ��Ώۂ̐ݒ�
        // ���Ɍ������Ă���I�u�W�F�N�g�̏ꍇ
        if (connectObjectCollider_List.Contains(breakerCollider))
        {
            return;
        }

        connectObjectCollider_List.Add(breakerCollider);

        // ���������I�u�W�F�N�g�Ԃ̏Փ˔���̖�����
        Physics.IgnoreCollision(myCollider, breakerCollider, true);

        DecideConnectPosition(breaker);

        FixedJoint fixedJoint = this.gameObject.AddComponent<FixedJoint>();
        fixedJoint.connectedBody = breaker.GetRigidbody();

        isConnect = true;

        Pierce pierce = breakerCollider.GetComponent<Pierce>();
        pierce.onDisconnectEvent.AddListener(DisConnect);

        // �������ɌĂяo�����C�x���g���Ăяo��
        onConnectEvent?.Invoke();  
    }

    /// <summary>
    /// �h�ˑ����ɂ�錋���̉���
    /// </summary>
    /// <param name="breaker">�󂷂��̃N���X</param>
    /// <returns></returns>
    private void DisConnect(Collider breakerCollider)
    {
        // �I�u�W�F�N�g�̓����̈ˑ��Ώۂ̉���
        FixedJoint[] fixedJoint_Array = this.gameObject.GetComponents<FixedJoint>();
        Rigidbody rigidbody = breakerCollider.GetComponent<Rigidbody>();

        // FixedJoint��S�Ă�������Ώۂ̂ݑI��
        foreach (FixedJoint fixedJoint in fixedJoint_Array)
        {
            if (fixedJoint.connectedBody == rigidbody)
            {
                Destroy(fixedJoint);
            }
        }

        connectObjectCollider_List.Remove(breakerCollider);

        if (connectObjectCollider_List.Count <= 0) isConnect = false;
        // ���������I�u�W�F�N�g�Ԃ̏Փ˔���̗L����
        Physics.IgnoreCollision(this.gameObject.GetComponent<Collider>(), breakerCollider, false);
    }

    /// <summary>
    /// �h�ˑ����ɂ�錋���̉���
    /// </summary>
    /// <param name="breaker">�󂷂��̃N���X</param>
    /// <returns></returns>
    private void DisconnectAll(Collider myCollider)
    {
        if (connectObjectCollider_List.Count <= 0) return;

        // FixedJoint��S�Ď擾���A
        FixedJoint[] fixedJoint_List = this.gameObject.GetComponents<FixedJoint>();

        // �I�u�W�F�N�g�̓����̈ˑ��Ώۂ̉���
        foreach (FixedJoint fixedJoint in fixedJoint_List)
        {
            Destroy(fixedJoint);
        }

        isConnect = false;
        // ���������I�u�W�F�N�g�Ԃ̏Փ˔���̗L����
        foreach (Collider connectObjectCollider in connectObjectCollider_List)
        {
            Physics.IgnoreCollision(myCollider, connectObjectCollider, false);
        }

        connectObjectCollider_List.Clear();
    }
}