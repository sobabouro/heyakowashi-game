using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slash : MonoBehaviour
{
    // 残り切断可能回数
    [SerializeField]
    private int numberOfCanSlash = 2;

    [SerializeField, Tooltip("切断面用のマテリアル")]
    Material surfaceMat;

    // 切断クラスの呼び出し時にはじめに呼び出され、ActSubdivideに切断させる
    public void CallSlash(Breaker breaker)
    {
        if (numberOfCanSlash <= 0)
        {

        }
        else
        {
            ActSubdivide.Subdivide(breaker.gameObject, breaker.GetCutter(), surfaceMat);
        }
    }
}
