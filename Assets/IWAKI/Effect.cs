using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour
{
    [SerializeField, Tooltip("発生させるエフェクト(パーティクル)")]
    private ParticleSystem particle;

    /// <summary>
    /// 衝突した時
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        // 当たった相手が"BreakableObject"タグを持っていたら
        if (collision.gameObject.tag == "BreakableObject")
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
