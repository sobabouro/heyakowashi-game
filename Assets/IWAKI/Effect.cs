using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour
{
    //<summary>元となったプログラム
    //</summary>

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

    //[System.Serializable]
    //public struct WeaponEffect
    //{
    //    public GameObject weapon; // 武器オブジェクト
    //    public ParticleSystem particle; // その武器に対応するエフェクト
    //}

    //[SerializeField, Tooltip("武器ごとに異なるエフェクトの設定")]
    //private List<WeaponEffect> weaponEffects = new List<WeaponEffect>();

    //[SerializeField, Tooltip("エフェクトの出現位置をずらすオフセット")]
    //private Vector3 offsetPosition; // インスペクターで指定できるオフセット

    //private ParticleSystem overrideParticle; // UnityEventで設定するパーティクルプレハブ

    ////[SerializeField]
    ////private ParticleSystem pDestroy; // 破棄時のエフェクト

    ///// <summary>
    ///// UnityEvent経由でパーティクルプレハブを選択するメソッド
    ///// </summary>
    ///// <param name="particlePrefab">選択するパーティクルプレハブ</param>
    //public void SetParticle(GameObject particlePrefab)
    //{
    //    if (particlePrefab != null)
    //    {
    //        overrideParticle = particlePrefab.GetComponent<ParticleSystem>();
    //    }
    //}

    ///// <summary>
    ///// 衝突した時にエフェクトを発生させる
    ///// </summary>
    ///// <param name="collision"></param>
    //private void OnCollisionEnter(Collision collision)
    //{
    //    // インスペクターで設定された武器ごとのパーティクルを検索
    //    foreach (var weaponEffect in weaponEffects)
    //    {
    //        if (weaponEffect.weapon == collision.gameObject)
    //        {
    //            // 衝突位置と回転を取得
    //            ContactPoint contact = collision.contacts[0];
    //            Vector3 hitPosition = contact.point + offsetPosition;
    //            Quaternion hitRotation = Quaternion.LookRotation(contact.normal);

    //            // 優先度: UnityEventで設定されたパーティクル
    //            if (overrideParticle != null)
    //            {
    //                InstantiateAndPlayParticle(overrideParticle, hitPosition, hitRotation);
    //            }
    //            else
    //            {
    //                InstantiateAndPlayParticle(weaponEffect.particle, hitPosition, hitRotation);
    //            }
    //            return;
    //        }
    //    }
    //}

    ///// <summary>
    ///// 指定されたパーティクルをインスタンス化して再生する
    ///// </summary>
    ///// <param name="particle">発生させるパーティクル</param>
    ///// <param name="position">エフェクトの位置</param>
    ///// <param name="rotation">エフェクトの回転</param>
    //private void InstantiateAndPlayParticle(ParticleSystem particle, Vector3 position, Quaternion rotation)
    //{
    //    if (particle != null)
    //    {
    //        ParticleSystem newParticle = Instantiate(particle);
    //        newParticle.transform.position = position;
    //        newParticle.transform.rotation = rotation;
    //        newParticle.transform.SetParent(this.transform);
    //        newParticle.Play();

    //        // 一定時間後に削除
    //        Destroy(newParticle.gameObject, newParticle.main.duration);
    //    }
    //}

    ////private void OnDestroy()
    ////{
    ////    ParticleSystem dParticle = Instantiate(pDestroy);
    ////    dParticle.Play();
    ////}

    [System.Serializable]
    public struct WeaponEffect
    {
        public GameObject weapon; // 武器オブジェクト
        public ParticleSystem particle; // その武器に対応するエフェクト
    }

    [SerializeField, Tooltip("武器ごとに異なるエフェクトの設定")]
    private List<WeaponEffect> weaponEffects = new List<WeaponEffect>();

    [SerializeField, Tooltip("キューブが壊れた時のエフェクト")]
    private ParticleSystem destroyParticle; // キューブ破壊時の専用パーティクル

    [SerializeField, Tooltip("エフェクトの出現位置をずらすオフセット")]
    private Vector3 offsetPosition; // インスペクターで指定できるオフセット

    private ParticleSystem overrideParticle; // UnityEventで設定するパーティクルプレハブ

    /// <summary>
    /// UnityEvent経由でパーティクルプレハブを選択するメソッド
    /// </summary>
    /// <param name="particlePrefab">選択するパーティクルプレハブ</param>
    public void SetParticle(GameObject particlePrefab)
    {
        if (particlePrefab != null)
        {
            overrideParticle = particlePrefab.GetComponent<ParticleSystem>();
        }
    }

    /// <summary>
    /// オブジェクトが破壊される直前にエフェクトを発生させる
    /// </summary>
    private void OnDestroy()
    {
        if (destroyParticle != null)
        {
            Vector3 destroyPosition = transform.position + offsetPosition;
            InstantiateAndPlayParticle(destroyParticle, destroyPosition, Quaternion.identity, false);
        }
    }

    /// <summary>
    /// 衝突した時にエフェクトを発生させる
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        // インスペクターで設定された武器ごとのパーティクルを検索
        foreach (var weaponEffect in weaponEffects)
        {
            if (weaponEffect.weapon == collision.gameObject)
            {
                // 衝突位置と回転を取得
                ContactPoint contact = collision.contacts[0];
                Vector3 hitPosition = contact.point + offsetPosition;
                Quaternion hitRotation = Quaternion.LookRotation(contact.normal);

                // 優先度: UnityEventで設定されたパーティクル
                if (overrideParticle != null)
                {
                    InstantiateAndPlayParticle(overrideParticle, hitPosition, hitRotation, true);
                }
                else
                {
                    InstantiateAndPlayParticle(weaponEffect.particle, hitPosition, hitRotation, true);
                }
                return;
            }
        }
    }

    /// <summary>
    /// 指定されたパーティクルをインスタンス化して再生する
    /// </summary>
    /// <param name="particle">発生させるパーティクル</param>
    /// <param name="position">エフェクトの位置</param>
    /// <param name="rotation">エフェクトの回転</param>
    /// <param name="setParent">エフェクトがオブジェクトに追従するかどうか</param>
    private void InstantiateAndPlayParticle(ParticleSystem particle, Vector3 position, Quaternion rotation, bool setParent)
    {
        if (particle != null)
        {
            ParticleSystem newParticle = Instantiate(particle);
            newParticle.transform.position = position;
            newParticle.transform.rotation = rotation;

            // 親を設定するかどうかを引数で制御
            if (setParent)
            {
                newParticle.transform.SetParent(this.transform);
            }

            newParticle.Play();

            // 一定時間後に削除
            Destroy(newParticle.gameObject, newParticle.main.duration);
        }
    }
}
