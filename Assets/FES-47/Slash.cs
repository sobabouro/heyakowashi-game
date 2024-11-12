using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class Slash : MonoBehaviour
{
    [SerializeField, Tooltip("残り切断可能回数")]
    private int _numberOfCanSlash = 2; 
    [SerializeField, Tooltip("切断面用のマテリアル")]
    private Material _surfaceMat; 
    [SerializeField, Tooltip("切断された後のオブジェクト")]
    private GameObject _dividedObjectPrefab;

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
        /*if (_numberOfCanSlash <= 0)
        {
            Destroy(this.gameObject);
            // 破壊時のイベントを呼び出す
            onBreakEvent?.Invoke();
        }
        else
        {
            // 生成したオブジェクトと干渉しないようにColliderを無効化
            this.gameObject.GetComponent<Collider>().enabled = false;
            (Mesh mesh1, Mesh mesh2) = ActSubdivide.Subdivide(this.gameObject, breaker.GetCutter());
            // 切断された後のオブジェクトを生成する
            CreateDividedObject(transform.position, mesh1);
            CreateDividedObject(transform.position, mesh2); 
            
            Destroy(this.gameObject);
            // 切断時のイベントを呼び出す
            onSlashEvent?.Invoke();
        }*/
    }

    /// <summary>
    /// 切断された後のオブジェクトを生成する
    /// </summary>
    /// <param name="originPosition">元オブジェクトの座標</param>
    /// <param name="newMesh">作成したメッシュ</param>
    /// <returns></returns>
    public void CreateDividedObject(Vector3 originPosition, Mesh newMesh)
    {
        GameObject dividedObject = Instantiate(_dividedObjectPrefab, originPosition, Quaternion.identity);
        Mesh mesh = dividedObject.GetComponent<Mesh>();
        // ここで作成したメッシュを代入
        // 座標を調整したりetc
    }
}
