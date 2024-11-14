using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

// breakable.cs で定義する
// public enum Type { plane, slash, crash, pierce }

public class Breaker : MonoBehaviour
{
    // 固有ステータス
    [SerializeField, Tooltip("基礎攻撃力")] private int _baseATK; // 基礎攻撃力
    [SerializeField, Tooltip("属性")] private Type _type;         // 属性

    // 計算用
    private float _velocity_threshold = 10; // ダメージが発生するために必要な最低限の速度
    private Rigidbody _rigidbody;          // 速度を取得するためのRigidbody
    // 移動方向を取得するための
    private Vector3 _moveDirection;  // 衝突時の移動方向;
    private Plane _cutter;         // 切断する平面;

    public UnityEvent onAttackEvent;

    // アクセサ
    public Type Type { get { return _type; } }

    public void SetRigidbody(Rigidbody rigidbody)
    {
        _rigidbody = rigidbody;
    }
    public Rigidbody GetRigidbody()
    {
        return _rigidbody;
    }
    public Plane GetCutter()
    {
        return _cutter;
    }
    public Vector3 GetMoveDirection()
    {
        return _moveDirection;
    }

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// 最終攻撃職を計算する。
    /// </summary>
    /// <param name="other_velocity">衝突相手の速度</param>
    private int CalcATK(float relative_vVelocity)
    {
        int finalATK = (int)(_baseATK * relative_vVelocity * 0.1);
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
        if (collision.contactCount == 0)
        {
            Debug.Log("collision.contactCount == 0");
            return;
        }
        float relative_vVelocity = collision.relativeVelocity.magnitude;
        if (relative_vVelocity < _velocity_threshold) return;

        // 衝突時の移動方向を保存
        _moveDirection = collision.relativeVelocity.normalized;
        // 断面をワールド座標で設定
        _cutter = CalcCutterPlane(collision.contacts[0].point);

        // 衝突相手の速度を取得
        int finalATK = CalcATK(relative_vVelocity);

        // 攻撃時のイベント発生
        onAttackEvent?.Invoke();

        // 相手に攻撃
        breakable.ReciveAttack(finalATK, this);

        // Debug.Log($"Attack!: {this.gameObject.name} to {breakable.gameObject.name}, finalATK: {finalATK}, velocity: {otherRigitbody.velocity}, {_rigidbody.velocity}");
    }

    /// <summary>
    /// カッター（切断する平面）を作成する
    /// </summary>
    /// <param name="worldPoint">平面の座標</param>
    private Plane CalcCutterPlane(Vector3 worldPoint)
    {
        // カッターの法線ベクトルをワールド空間で計算
        Vector3 worldNormal = Vector3.Cross(transform.forward.normalized, _moveDirection).normalized;
        Debug.DrawRay(worldPoint, worldNormal, Color.green, 2, false);
        // 平面の距離を計算：平面の法線ベクトルからワールド空間の任意の点への距離
        float worldDistance = Vector3.Dot(worldNormal, worldPoint);
        // 断面を相手のワールド座標で設定
        return new Plane(worldNormal, worldDistance);
    }

}
