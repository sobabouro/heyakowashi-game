using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour
{
    //[SerializeField, Tooltip("発生させるエフェクト(パーティクル)")]
    //private ParticleSystem particle;

    //[SerializeField, Tooltip("エフェクトを発生させる対象のオブジェクトリスト")]
    //private List<GameObject> targetObjects; // インスペクターで指定可能なオブジェクトリスト

    ///// <summary>
    ///// 衝突した時
    ///// </summary>
    ///// <param name="collision"></param>
    //private void OnCollisionEnter(Collision collision)
    //{
    //    // 衝突したオブジェクトが指定リストに含まれている場合
    //    if (targetObjects.Contains(collision.gameObject))
    //    {
    //        // 衝突地点を取得
    //        ContactPoint contact = collision.contacts[0];
    //        Vector3 hitPosition = contact.point;
    //        Quaternion hitRotation = Quaternion.LookRotation(contact.normal);

    //        // パーティクルシステムのインスタンスを生成
    //        ParticleSystem newParticle = Instantiate(particle);
    //        // 衝突地点にパーティクルを配置
    //        newParticle.transform.position = hitPosition;
    //        // 衝突の向きに回転させる
    //        newParticle.transform.rotation = hitRotation;
    //        // パーティクルシステムをGameObjectに追従させるために親を設定する
    //        newParticle.transform.SetParent(this.transform);
    //        // パーティクルを発生させる
    //        newParticle.Play();

    //        // 一定時間後に削除する
    //        Destroy(newParticle.gameObject, newParticle.main.duration);
    //    }
    //}

    [System.Serializable]
    public struct WeaponEffect
    {
        public GameObject weapon; // 武器オブジェクト
        public ParticleSystem particle; // その武器に対応するエフェクト
    }

    [SerializeField, Tooltip("武器ごとに異なるエフェクトの設定")]
    private List<WeaponEffect> weaponEffects = new List<WeaponEffect>();

    [SerializeField, Tooltip("エフェクトの出現位置をずらすオフセット")]
    private Vector3 offsetPosition;

    // Dictionary に変換して検索を高速化
    private Dictionary<GameObject, ParticleSystem> effectDictionary;

    private void Awake()
    {
        // weaponEffectsリストからDictionaryを初期化
        effectDictionary = new Dictionary<GameObject, ParticleSystem>();
        foreach (var weaponEffect in weaponEffects)
        {
            if (weaponEffect.weapon != null && weaponEffect.particle != null)
            {
                effectDictionary[weaponEffect.weapon] = weaponEffect.particle;
            }
        }
    }

    /// <summary>
    /// エフェクトを発生させるメソッド（UnityEventで使用）
    /// </summary>
    /// <param name="useHitPosition">trueなら衝突位置、falseならオブジェクトの中心からエフェクトを発生させる</param>
    public void TriggerEffect(bool useHitPosition)
    {
        // 衝突した武器に対応するエフェクトが設定されているか確認
        foreach (var entry in effectDictionary)
        {
            // エフェクトを発生させる条件を満たす武器があれば
            if (entry.Key.activeInHierarchy) // 特定の武器がアクティブであるかを判定
            {
                Vector3 effectPosition = useHitPosition ? entry.Key.transform.position + offsetPosition : transform.position + offsetPosition;
                Quaternion effectRotation = Quaternion.identity; // 任意の回転

                // パーティクルシステムのインスタンスを生成
                ParticleSystem newParticle = Instantiate(entry.Value);
                newParticle.transform.position = effectPosition;
                newParticle.transform.rotation = effectRotation;
                newParticle.transform.SetParent(this.transform);
                newParticle.Play();

                // 一定時間後に削除
                Destroy(newParticle.gameObject, newParticle.main.duration);

                // エフェクトを発生させたら終了
                break;
            }
        }
    }

}
