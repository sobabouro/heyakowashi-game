using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// breakable.cs で定義する
// public enum Type { plane, slash, crash, pierce }

public class Breaker : MonoBehaviour
{
    // 自分の親オブジェクト
    [SerializeField]
    private Transform _container = null;

    [SerializeField, Tooltip("基礎攻撃力")]
    private int _baseATK = default;
    [SerializeField, Tooltip("属性")]
    private Type _type = Type.plane;
    // 速度を取得するためのRigitbody
    [SerializeField]
    private Rigidbody my_rigidbody;
    // ダメージが発生するために必要な最低限の速度
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
    /// 攻撃するメソッド。オブジェクトと衝突時に呼び出す。
    /// </summary>
    /// <param name="collision">衝突データ全般</param>
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
