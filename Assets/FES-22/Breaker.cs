using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

// breakable.cs で定義する
// public enum Type { plane, slash, crash, pierce }

public class Breaker : MonoBehaviour
{
    // 固有ステータス
    [SerializeField, Tooltip("基礎攻撃力")] private int _baseATK; // 基礎攻撃力
    [SerializeField, Tooltip("属性")] private Type _type;         // 属性

    // 計算用
    private float _velocity_threshold = 0; // ダメージが発生するために必要な最低限の速度
    private Rigidbody _rigidbody;          // 速度を取得するためのRigidbody
    // 移動方向を取得するための
    private Vector3 _prePosition;  // 1フレーム前の座標;
    private Vector3 _nowPosition;  // 現在の座標;
    private Plane _cutter;         // 切断する平面;

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

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }
    void FixedUpdate()
    {
        // 座標の更新
        _prePosition = _nowPosition;
        _nowPosition = transform.position;
    }

    /// <summary>
    /// 最終攻撃職を計算する。
    /// </summary>
    /// <param name="other_velocity">衝突相手の速度</param>
    private int CalcATK(Vector3 other_velocity)
    {
        // 相対速度を求める
        float velocity = (_rigidbody.velocity - other_velocity).magnitude;
        if (velocity < _velocity_threshold) velocity = 0;
        int finalATK = (int)(_baseATK * velocity);
        return finalATK;
    }

    //  �ؒf�N���X�p�̐ؒf���ʌv�Z
    private void CalcCutter(Collision collision)
    {
        // �Փ˓_�̃��[���h���W���擾
        ContactPoint contactPoint = collision.contacts[0];
        Vector3 collisionPositionWorld = contactPoint.point;

        // �Փˑ���̃��[�J�����W�ɕϊ�
        Vector3 collisionPositionLocal = collision.transform.InverseTransformPoint(collisionPositionWorld);

        // �J�b�^�[�̖@���x�N�g�������[���h��ԂŌv�Z
        Vector3 worldNormal = Vector3.Cross(transform.forward.normalized, prePos - transform.position).normalized;

        // ���ʂ̋������v�Z�F���ʂ̖@���x�N�g�����烏�[���h��Ԃ̔C�ӂ̓_�i�Ⴆ�� collisionPositionWorld�j�ւ̋���
        float worldDistance = Vector3.Dot(worldNormal, collisionPositionWorld);

        // �J�b�^�[�̕��ʂ����[���h���W�Őݒ�
        cutter = new Plane(worldNormal, worldDistance);
    }

    /// <summary>
    /// 攻撃するメソッド。オブジェクトと衝突時に呼び出す。
    /// </summary>
    /// <param name="collision">衝突データ全般</param>
    public void Attack(Collision collision)
    {
        Breakable breakable = collision.gameObject.GetComponent<Breakable>();
        
        if (breakable == null) return;

        // 衝突点のワールド座標を取得
        Vector3 collisionPositionWorld = collision.contacts[0].point;
        // 衝突相手のローカル座標に変換
        Vector3 collisionPositionLocal = collision.transform.InverseTransformPoint(collisionPositionWorld);
        // 断面を相手のローカル座標で設定
        CalcCutterPlane(collisionPositionLocal);

        // 衝突相手の速度を取得
        Rigidbody otherRigitbody = collision.gameObject.GetComponent<Rigidbody>();
        int finalATK = CalcATK(otherRigitbody.velocity);
<<<<<<< HEAD
        
        breakable.ReciveAttack(finalATK, this);

        // �ؒf�N���X�p�̕��ʌv�Z�Ăяo��
        CalcCutter(collision);

        Debug.Log("Attack! : " + this.gameObject + " to " + breakable + " : " + finalATK + " : " + otherRigitbody.velocity + " : " + my_rigidbody.velocity);
=======

        // 相手に攻撃
        breakable.ReciveAttack(finalATK, this);

        Debug.Log($"Attack!: {this.gameObject.name} to {breakable.gameObject.name}, finalATK: {finalATK}, velocity: {otherRigitbody.velocity}, {_rigidbody.velocity}");
>>>>>>> origin/main
    }

    /// <summary>
    /// カッター（切断する平面）を作成する
    /// </summary>
    /// <param name="collision">衝突データ全般</param>
    private void CalcCutterPlane(Vector3 point)
    {
        // 断面を相手のローカル座標で設定
        _cutter = new Plane(Vector3.Cross(transform.forward.normalized, _prePosition - _nowPosition).normalized, point);
    }

}
