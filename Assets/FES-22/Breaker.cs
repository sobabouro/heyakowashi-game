using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Type
{
    plane,
    slash,
    crash,
    pierce
}

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
    private Rigidbody my_rigitbody;
    // ダメージが発生するために必要な最低限の速度
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
    /// 攻撃するメソッド。オブジェクトと衝突時に呼び出す。
    /// </summary>
    /// <param name="breakableObject">壊されるオブジェクト（衝突相手）</param>
    public void Attack(GameObject breakableObject)
    {
        Rigidbody otherRigitbody = breakableObject.GetComponent<Rigidbody>();
        int finalATK = CalcATK(otherRigitbody.velocity);
        Breakable breakable = breakableObject.GetComponent<Breakable>();
        breakable.ReceiveAttack(finalATK, this);
    }

}
