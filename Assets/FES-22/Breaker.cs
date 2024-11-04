using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// breakable.cs で定義する
// public enum Type { plane, slash, crash, pierce }

public class Breaker : MonoBehaviour
{
    [SerializeField, Tooltip("基礎攻撃力")]
    private int _baseATK = default;
    [SerializeField, Tooltip("属性")]
    private Type _type = Type.plane;
    // 速度を取得するためのRigidbody
    [SerializeField]
    private Rigidbody my_rigidbody;
    // ダメージが発生するために必要な最低限の速度
    [SerializeField]
    private float _velocity_threshold = 0;

    // 動く方向で切断する場合に必要な現在と一フレ前の座標
    private Vector3 prePos = Vector3.zero;
    private Vector3 prePos2 = Vector3.zero;

    private Plane cutter;

    public Type Type { get { return _type; } }

    private void Start()
    {

    }

    void FixedUpdate()
    {
        prePos = prePos2;
        prePos2 = transform.position;
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
        Breakable breakable = collision.gameObject.GetComponent<Breakable>();
        
        if (breakable == null) return;

        Rigidbody otherRigitbody = collision.gameObject.GetComponent<Rigidbody>();
        int finalATK = CalcATK(otherRigitbody.velocity);

        // 衝突点のワールド座標を取得
        ContactPoint contactPoint = collision.contacts[0];
        Vector3 collisionPositionWorld = contactPoint.point;

        // 衝突相手のローカル座標に変換
        Vector3 collisionPositionLocal = collision.transform.InverseTransformPoint(collisionPositionWorld);

        // カッターの平面を相手のローカル座標で設定
        cutter = new Plane(Vector3.Cross(transform.forward.normalized, prePos - transform.position).normalized, collisionPositionLocal);

        breakable.ReciveAttack(finalATK, this);

        Debug.Log("Attack! : " + this.gameObject + " to " + breakable + " : " + finalATK + " : " + otherRigitbody.velocity + " : " + my_rigidbody.velocity);
    }

    public void SetRigidbody(Rigidbody rigidbody)
    {
        my_rigidbody = rigidbody;
    }

    public Rigidbody GetRigidbody()
    {
        return my_rigidbody;
    }

    public Plane GetCutter()
    {
        return cutter;
    }
}
