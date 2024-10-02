using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public enum Type { plane, slash, crash, pierce }

public class Breakable : MonoBehaviour
{
    [SerializeField, Tooltip("�ϋv�l")]
    private int durability = default;
    [Header("�����ϐ�")]
    [SerializeField, Tooltip("�ؒf�ϐ�")]
    private int slashResist = default;
    [SerializeField, Tooltip("�Ռ��ϐ�"),]
    private int crashResist = default;
    [SerializeField, Tooltip("�ђʑϐ�"),]
    private int pierceResist = default;
    [SerializeField, Tooltip("�X�R�A")]
    private int _score = default;

    // �����ϐ��̎���
    private Dictionary<Type, int> resists = new Dictionary<Type, int>();
    // �������Ă���Ƃ��̌��������Breaker�N���X
    private Container container = null;

    [SerializeField]
    private float maxInterval = default;
    private float nowInterval = 0;
    private bool inInterval = false;

    private void Start()
    {
        resists.Add(Type.slash, slashResist);
        resists.Add(Type.crash, crashResist);
        resists.Add(Type.pierce, pierceResist);
        resists.Add(Type.plane, 0);
    }

    private void Update()
    {
        CalcInterval();
    }

    /// <summary>
    /// �A���ōU�����󂯂Ȃ��悤�ɂ���C���^�[�o��
    /// </summary>
    private void CalcInterval()
    {
        if (inInterval)
        {
            nowInterval += Time.deltaTime;
            if (nowInterval > maxInterval)
            {
                nowInterval = 0;
                inInterval = false;
            }
        }
    }

    /// <summary>
    /// �U�����ꂽ���ɌĂяo�����\�b�h�B
    /// </summary>
    /// <param name="receivedATK">�󂯂�U����</param>
    /// <param name="breaker">�U���������̏��</param>
    /// <returns></returns>
    public bool ReciveAttack(int receivedATK, Breaker breaker)
    {
        if (inInterval) return false;
        inInterval = true;

        int damage = CalcDamage(receivedATK, breaker.Type);
        Debug.Log($"damage: {damage}");
        durability -= damage;
        Debug.Log($"durability: {durability}");
        if (durability < 0)
        {
            Break(breaker);
            return true;
        }
        return false;
    }

    /// <summary>
    /// �ϋv�l���O�ɂȂ����Ƃ��̃��\�b�h
    /// </summary>
    /// <param name="breaker">`�U���������̏��</param>
    private void Break(Breaker breaker)
    {
        Debug.Log("Break");
        /*addScore(_score);*/
        if (container != null)
        {
            this.gameObject.transform.parent.gameObject.GetComponent<Container>().SetMainRegister();
        }
        switch (breaker.Type)
        {
            case Type.slash:
                // Slash�N���X���Ăяo��
                Destroy(this.gameObject);
                break;
            case Type.crash:
                Debug.Log("Destroy! : " + this.gameObject);
                // Crash�N���X���Ăяo��
                Destroy(this.gameObject);
                break;
            case Type.pierce:
                // Pierce�N���X���Ăяo��
                container = breaker.GetContainer();
                durability = this.gameObject.GetComponent<Pierce>().Connect(breaker);
                break;
            default:
                break;
        }
    }


    /// <summary>
    /// �^����ꂽ�U���͂Ƒ����A���g�̑ϐ��A�ŏI�I�ȃ_���[�W�̒l���v�Z����B
    /// </summary>
    /// <param name="receivedATK">�󂯂�U����</param>
    /// <param name="attackType">�󂯂�U���̑���</param>
    /// <returns></returns>
    private int CalcDamage(int receivedATK, Type attackType)
    {
        int damage = receivedATK - resists[attackType];
        if (damage < 0) damage = 0;
        return damage;
    }

}
