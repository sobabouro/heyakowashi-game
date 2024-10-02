using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public enum Type { plane, slash, crash, pierce }

public class Breakable : MonoBehaviour
{
    [SerializeField, Tooltip("耐久値")]
    private int durability = default;
    [Header("属性耐性")]
    [SerializeField, Tooltip("切断耐性")]
    private int slashResist = default;
    [SerializeField, Tooltip("衝撃耐性"),]
    private int crashResist = default;
    [SerializeField, Tooltip("貫通耐性"),]
    private int pierceResist = default;
    [SerializeField, Tooltip("スコア")]
    private int _score = default;

    // 属性耐性の辞書
    private Dictionary<Type, int> resists = new Dictionary<Type, int>();
    // 結合しているときの結合相手のBreakerクラス
    private Container container = null;

    [SerializeField]
    private float maxInterval = default;
    private float nowInterval = 0;
    private bool inInterval = false;

    private void Start()
    {
        resists.Add(Type.slash, slashResist);
        resists.Add(Type.crash, crashResist);
        resists.Add(Type.pierce, pierceResist);
        resists.Add(Type.plane, 0);
    }

    private void Update()
    {
        CalcInterval();
    }

    /// <summary>
    /// 連続で攻撃を受けないようにするインターバル
    /// </summary>
    private void CalcInterval()
    {
        if (inInterval)
        {
            nowInterval += Time.deltaTime;
            if (nowInterval > maxInterval)
            {
                nowInterval = 0;
                inInterval = false;
            }
        }
    }

    /// <summary>
    /// 攻撃された時に呼び出すメソッド。
    /// </summary>
    /// <param name="receivedATK">受ける攻撃力</param>
    /// <param name="breaker">攻撃した側の情報</param>
    /// <returns></returns>
    public bool ReciveAttack(int receivedATK, Breaker breaker)
    {
        if (inInterval) return false;
        inInterval = true;

        int damage = CalcDamage(receivedATK, breaker.Type);
        Debug.Log($"damage: {damage}");
        durability -= damage;
        Debug.Log($"durability: {durability}");
        if (durability < 0)
        {
            Break(breaker);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 耐久値が０になり壊れるときのメソッド
    /// </summary>
    /// <param name="breaker">`攻撃した側の情報</param>
    private void Break(Breaker breaker)
    {
        Debug.Log("Break");
        /*addScore(_score);*/
        if (container != null)
        {
            this.gameObject.transform.parent.gameObject.GetComponent<Container>().SetMainRegister();
        }
        switch (breaker.Type)
        {
            case Type.slash:
                // Slashクラスを呼び出す
                Destroy(this.gameObject);
                break;
            case Type.crash:
                Debug.Log("Destroy! : " + this.gameObject);
                // Crashクラスを呼び出す
                Destroy(this.gameObject);
                break;
            case Type.pierce:
                // Pierceクラスを呼び出す
                container = breaker.GetContainer();
                durability = this.gameObject.GetComponent<Pierce>().Connect(breaker);
                break;
            default:
                break;
        }
    }


    /// <summary>
    /// 与えられた攻撃力と属性、自身の耐性、最終的なダメージの値を計算する。
    /// </summary>
    /// <param name="receivedATK">受ける攻撃力</param>
    /// <param name="attackType">受ける攻撃の属性</param>
    /// <returns></returns>
    private int CalcDamage(int receivedATK, Type attackType)
    {
        int damage = receivedATK - resists[attackType];
        if (damage < 0) damage = 0;
        return damage;
    }

}
