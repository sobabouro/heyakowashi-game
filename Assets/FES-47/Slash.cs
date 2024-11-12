using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Slash : MonoBehaviour
{
    [SerializeField, Tooltip("残り切断可能回数")]
    private int _numberOfCanSlash = 2;
    [SerializeField, Tooltip("切断面用のマテリアル")]
    private Material _cutSurfaceMaterial; 
    [SerializeField, Tooltip("切断された後のオブジェクト")]
    private GameObject _cutObjectPrefab;

    // オブジェクト切断時に呼び出すイベント登録
    public UnityEvent onSlashEvent; 
    // オブジェクト破壊時に呼び出すイベント登録
    public UnityEvent onBreakEvent;


    /// <summary>
    /// 切断クラスの呼び出し時にはじめに呼び出され、ActSubdivideに切断させる
    /// </summary>
    /// <param name="breaker">攻撃した側の情報</param>
    /// <returns></returns>
    public void CallSlash(Breaker breaker)
    {
        if (_numberOfCanSlash <= 0)
        {
            Destroy(this.gameObject);
            // 破壊時のイベントを呼び出す
            onBreakEvent?.Invoke();
        }
        else
        {
            // 生成したオブジェクトと干渉しないようにColliderを無効化
            this.gameObject.GetComponent<Collider>().enabled = false;

            Material[] materials = this.gameObject.GetComponent<MeshRenderer>().sharedMaterials;
            Material[] newMaterials;
            // 切断前のマテリアルが切断面用のマテリアルを持っていなければ，マテリアルを割り当てる
            Material lastMaterial = materials[materials.Length - 1];
            bool canAddNewMaterial = lastMaterial.name == _cutSurfaceMaterial?.name;
            if (canAddNewMaterial)
            {
                newMaterials = new Material[materials.Length + 1];
                materials.CopyTo(newMaterials, 0);
                newMaterials[materials.Length] = _cutSurfaceMaterial;
            }
            else
            {
                newMaterials = materials;
            }

            // 参照を取得
            Transform transform = this.gameObject.transform;
            Mesh mesh = this.gameObject.GetComponent<MeshFilter>().mesh;
            // 切断したオブジェクトのメッシュを計算する。
            (Mesh rightMesh, Mesh leftMesh) = ActSubdivide.Subdivide(mesh, transform, breaker.GetCutter(), canAddNewMaterial);

            // 失敗
            if (rightMesh == null || leftMesh == null)　return;

            // 切断された後のオブジェクトを生成する
            CreateCutObject(transform, rightMesh, newMaterials);
            CreateCutObject(transform, leftMesh, newMaterials); 

            Destroy(this.gameObject);
            // 切断時のイベントを呼び出す
            onSlashEvent?.Invoke();
        }
    }

    /// <summary>
    /// 切断された後のオブジェクトを生成する
    /// </summary>
    /// <param name="originTransform">元オブジェクトのTransform</param>
    /// <param name="newMesh">作成したメッシュ</param>
    /// <returns></returns>
    public void CreateCutObject(Transform originTransform, Mesh newMesh, Material[] newMaterials)
    {
        GameObject polygonInfo_subject = Instantiate(_cutObjectPrefab, originTransform.position, originTransform.rotation, null);

        // Meshの設定
        polygonInfo_subject.GetComponent<MeshFilter>().mesh = newMesh;

        // マテリアルの設定
        polygonInfo_subject.GetComponent<MeshRenderer>().sharedMaterials = newMaterials;

        // MeshColliderの設定
        MeshCollider meshCollider = polygonInfo_subject.GetComponent<MeshCollider>();
        if(meshCollider)
        {
            polygonInfo_subject.GetComponent<MeshCollider>().sharedMesh = newMesh;
        }
    }
}
