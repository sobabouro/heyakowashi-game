using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// breakable.cs �Œ�`����
// public enum Type { plane, slash, crash, pierce }

public class Breaker : MonoBehaviour
{
    // �����̐e�I�u�W�F�N�g
    [SerializeField]
    private Transform _container = null;

    [SerializeField, Tooltip("��b�U����")]
    private int _baseATK = default;
    [SerializeField, Tooltip("����")]
    private Type _type = Type.plane;
    // ���x���擾���邽�߂�Rigitbody
    [SerializeField]
    private Rigidbody my_rigitbody;
    // �_���[�W���������邽�߂ɕK�v�ȍŒ���̑��x
    [SerializeField]
    private float _velocity_threshold = 0;

    public Type Type { get { return _type; } }


    private int CalcATK(Vector3 other_velocity)
    {
        float velocity = (my_rigitbody.velocity - other_velocity).magnitude;
        if (velocity < _velocity_threshold) velocity = 0;
        int finalATK = (int)(_baseATK * my_rigitbody.velocity.magnitude);
        return finalATK;
    }

    /// <summary>
    /// �U�����郁�\�b�h�B�I�u�W�F�N�g�ƏՓˎ��ɌĂяo���B
    /// </summary>
    /// <param name="collision">�Փ˃f�[�^�S��</param>
    public void Attack(Collision collision)
    {
        Breakable breakable = collision.gameObject.GetComponent<Breakable>();
        if (breakable == null) return;

        Rigidbody otherRigitbody = collision.gameObject.GetComponent<Rigidbody>();
        int finalATK = CalcATK(otherRigitbody.velocity);
        breakable.ReciveAttack(finalATK, this);
    }

}
