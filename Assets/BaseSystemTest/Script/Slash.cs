using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Slash : MonoBehaviour
{
    // 残り切断可能回数
    [SerializeField]
    private int numberOfCanSlash = 2;

    // 切断後に呼び出されるプレハブ
    [SerializeField]
    private GameObject generatePrefab;

    [SerializeField, Tooltip("切断面用のマテリアル")]
    Material surfaceMat;

    // オブジェクト切断時に呼び出すイベント登録
    public UnityEvent onSlashEvent;

    // オブジェクト破壊時に呼び出すイベント登録
    public UnityEvent onBreakEvent;

    // 切断クラスの呼び出し時にはじめに呼び出され、ActSubdivideに切断させる
    public void CallSlash(Breaker breaker)
    {
        if (numberOfCanSlash <= 0)
        {
            Destroy(this.gameObject);

            // 破壊時に呼び出されるイベントを呼び出す
            onBreakEvent?.Invoke();
        }
        else
        {
            // ActSubdivide内でオブジェクトの破棄はおこなわれる（はず）
            ActSubdivide.Subdivide(breaker.gameObject, generatePrefab, breaker.GetCutter(), surfaceMat);

            /*
             ここで切断後に生成されるオブジェクトの残り切断可能回数を処理する
             */

            // 切断時に呼び出されるイベントを呼び出す
            onSlashEvent?.Invoke();
        }


    }

    public void SetNumberOfCanSlash(int numberOfCanSlash)
    {
        this.numberOfCanSlash = numberOfCanSlash;
    }
}
