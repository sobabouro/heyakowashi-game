using UnityEngine;

public class TitleExplosion : MonoBehaviour
{
    public GameObject titleText; // タイトルの親オブジェクト
    public float explosionForce = 500f; // 爆発力
    public float explosionRadius = 5f; // 爆発の範囲

    public void ExplodeTitle()
    {
        // タイトルの各文字にRigidbodyを追加して爆発させる
        foreach (Transform part in titleText.transform)
        {
            var rb = part.gameObject.AddComponent<Rigidbody>();
            rb.AddExplosionForce(explosionForce, titleText.transform.position, explosionRadius);
        }
    }
}
