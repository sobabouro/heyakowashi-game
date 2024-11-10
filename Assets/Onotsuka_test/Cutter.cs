using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cutter : MonoBehaviour {

    [SerializeField, Tooltip("切断面用のマテリアル")]
    Material surfaceMat;


    // 動く方向で切断する場合
    private Vector3 prePos = Vector3.zero;
    private Vector3 prePos2 = Vector3.zero;

    

    void FixedUpdate() {
        prePos = prePos2;
        prePos2 = transform.position;
    }

    private void OnCollisionEnter(Collision collision) {
        //var actSubdivide = collision.gameObject.GetComponent<ActSubdivide>();
        //if (actSubdivide == null) {
        //    return;
        //}

        // 衝突点のワールド座標を取得
        ContactPoint contactPoint = collision.contacts[0];
        Vector3 collisionPositionWorld = contactPoint.point;

        // 衝突したオブジェクトの名前と衝突点の座標をログに出力
        Debug.Log($"Collision detected with object: {collision.gameObject.name}");
        Debug.Log($"Collision position: {collisionPositionWorld}");

        //// 衝突相手のローカル座標に変換
        //Vector3 collisionPositionLocal = collision.transform.InverseTransformPoint(collisionPositionWorld);

        // カッターの平面を相手のワールド座標で設定
        var cutter = new Plane(Vector3.Cross(transform.forward.normalized, prePos - transform.position).normalized, collisionPositionWorld);

        ActSubdivide.Subdivide(collision.gameObject, cutter, surfaceMat);

        //MeshCut.CutMesh(collision.gameObject, collisionPositionWorld, cutter.normal, true, surfaceMat);

        Debug.Log("Subdivide method called.");
    }
}
