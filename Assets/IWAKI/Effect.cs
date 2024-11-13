using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour
{
    [SerializeField, Tooltip("発生させるエフェクト(パーティクル)")]
    private ParticleSystem particle;

    [SerializeField, Tooltip("エフェクトを発生させる対象のオブジェクトリスト")]
    private List<GameObject> targetObjects; // インスペクターで指定可能なオブジェクトリスト

    /// <summary>
    /// 衝突した時
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        // 衝突したオブジェクトが指定リストに含まれている場合
        if (targetObjects.Contains(collision.gameObject))
        {
            // 衝突地点を取得
            ContactPoint contact = collision.contacts[0];
            Vector3 hitPosition = contact.point;
            Quaternion hitRotation = Quaternion.LookRotation(contact.normal);

            // パーティクルシステムのインスタンスを生成
            ParticleSystem newParticle = Instantiate(particle);
            // 衝突地点にパーティクルを配置
            newParticle.transform.position = hitPosition;
            // 衝突の向きに回転させる
            newParticle.transform.rotation = hitRotation;
            // パーティクルシステムをGameObjectに追従させるために親を設定する
            newParticle.transform.SetParent(this.transform);
            // パーティクルを発生させる
            newParticle.Play();

            // 一定時間後に削除する
            Destroy(newParticle.gameObject, newParticle.main.duration);
        }
    }

}
