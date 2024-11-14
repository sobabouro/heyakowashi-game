using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cutter : MonoBehaviour {

    [SerializeField, Tooltip("切断面用のマテリアル")]
    Material surfaceMat;

    // 切断後に呼び出されるプレハブ
    [SerializeField]
    private GameObject generatePrefab;

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

        // 衝突相手のローカル座標に変換
        Vector3 collisionPositionLocal = collision.transform.InverseTransformPoint(collisionPositionWorld);

        // カッターの法線ベクトルをワールド空間で計算
        Vector3 worldNormal = Vector3.Cross(transform.forward.normalized, prePos - transform.position).normalized;

        // 衝突相手のローカル空間に法線ベクトルを変換
        Vector3 localNormal = collision.transform.InverseTransformDirection(worldNormal);

        // 平面の距離を計算：平面の法線ベクトルからワールド空間の任意の点（例えば collisionPositionWorld）への距離
        float worldDistance = Vector3.Dot(worldNormal, collisionPositionWorld);

        //// カッターの平面を相手のローカル座標で設定
        //var cutter = new Plane(localNormal, collisionPositionLocal);

        // カッターの平面をワールド座標で設定
        var cutter = new Plane(worldNormal, worldDistance);

        ActSubdivide.Subdivide(collision.gameObject, cutter, surfaceMat);
    }
}
