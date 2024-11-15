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
    [SerializeField, Tooltip("速度係数")] private float _velocity_coefficient = 1; // 攻撃力に影響する速度の割合
    [SerializeField, Tooltip("ダメージが発生するために必要な最低限の速度")]
    private float _minimumSpeedNeededForDamage = 3.0f; // ダメージが発生するために必要な最低限の速度
    [Header("突属性")]
    [SerializeField, Tooltip("突属性を持つ？")] private bool _canPierce; // 突属性を持つ？
    [SerializeField, Tooltip("突属性の向き")] private Vector3 _pierceDirection; // 突属性の向き
    [SerializeField, Tooltip("突属性の向きの一致度（内積）")] private float _matchDegreeOfOrientation = 0.866f; // 突属性の向きの一致度（内積）
    [Header("斬属性")]
    [SerializeField, Tooltip("斬属性を持つ？")] private bool _canSlash; // 斬属性を持つ？

    [Header("イベント登録")]
    public UnityEvent onAttackEvent;

    // 計算用
    private Rigidbody _rigidbody;    // 速度を取得するためのRigidbody
    private Vector3 _moveDirection;  // 移動方向
    private Vector3 _contactPoint;  // 衝突座標


    // アクセサ
    public void SetRigidbody(Rigidbody rigidbody)
    {
        _rigidbody = rigidbody;
    }
    public Rigidbody GetRigidbody()
    {
        return _rigidbody;
    }

    public Vector3 GetMoveDirection()
    {
        return _moveDirection;
    }
    public Vector3 GetContactPoint()
    {
        return _contactPoint;
    }

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// 最終攻撃職を計算する。
    /// </summary>
    /// <param name="other_velocity">衝突相手の速度</param>
    private int CalcATK(float relative_velocity)
    {
        int finalATK = (int)(_baseATK * relative_velocity * _velocity_coefficient);
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

        // 衝突の速度を取得
        Vector3 relative_velocity = -collision.relativeVelocity;
        float relative_velocity_magnitude = relative_velocity.magnitude;
        // 最低速度未満ならダメージは発生しない
        if (relative_velocity_magnitude < _minimumSpeedNeededForDamage) return;

        // 攻撃時のイベント発生
        onAttackEvent?.Invoke();
        int finalATK = CalcATK(relative_velocity_magnitude);

        // 衝突時の移動方向を保存
        _moveDirection = relative_velocity.normalized;

        // 衝突位置を取得
        if (collision.contactCount > 0)
        {
            _contactPoint = collision.contacts[0].point;
        } else {
            // クソみたいなバグ
            _contactPoint = collision.collider.ClosestPoint(transform.position);
        }

        // 基本は壊属性
        Type type = Type.crash;
        // 斬属性なら斬属性優先
        if (_canSlash)
        {
            type = Type.slash;
        }
        // 突属性なら突属性優先
        if (_canPierce)
        {
            // ローカル座標系での移動方向
            Vector3 local_moveDirection = transform.rotation * _moveDirection;
            // 突属性の向きと30度以内の方向なら突属性
            if (Mathf.Abs(Vector3.Dot(_pierceDirection, local_moveDirection)) > _matchDegreeOfOrientation)
            {
                type = Type.pierce;
            }
        }

        // 相手に攻撃
        breakable.ReciveAttack(this, finalATK, type);

        // Debug.Log($"Attack!: {this.gameObject.name} to {breakable.gameObject.name}, finalATK: {finalATK}, relative_velocity: {relative_velocity}");
    }

}
