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
    private Rigidbody my_rigidbody;
    // �_���[�W���������邽�߂ɕK�v�ȍŒ���̑��x
    [SerializeField]
    private float _velocity_threshold = 0;

    public Type Type { get { return _type; } }

    private void Start()
    {
        
    }


    private int CalcATK(Vector3 other_velocity)
    {
        float velocity = (my_rigidbody.velocity - other_velocity).magnitude;
        if (velocity < _velocity_threshold) velocity = 0;
        int finalATK = (int)(_baseATK * velocity);
        return finalATK;
    }

    /// <summary>
    /// �U�����郁�\�b�h�B�I�u�W�F�N�g�ƏՓˎ��ɌĂяo���B
    /// </summary>
    /// <param name="collision">�Փ˃f�[�^�S��</param>
    public void Attack(Collision collision)
    {
        Container container = collision.gameObject.GetComponent<Container>();
        Breakable breakable;
        if (container != null)
        {
            breakable = container.GetRegisteredObject().GetComponent<Breakable>();
        }
        else
        {
            breakable = collision.gameObject.GetComponent<Breakable>();
        }
        
        if (breakable == null) return;

        Rigidbody otherRigitbody = collision.gameObject.GetComponent<Rigidbody>();
        int finalATK = CalcATK(otherRigitbody.velocity);
        breakable.ReciveAttack(finalATK, this);

        Debug.Log("Attack! : " + this.gameObject + " to " + breakable + " : " + finalATK + " : " + otherRigitbody.velocity + " : " + my_rigidbody.velocity);
    }

    public Transform GetContainer()
    {
        return _container;
    }

    public void SetRigidbody(Rigidbody rigidbody)
    {
        my_rigidbody = rigidbody;
    }
}
