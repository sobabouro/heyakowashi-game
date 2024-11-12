using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public enum Type { plane, slash, crash, pierce }

public class Breakable : MonoBehaviour
{
    // 固有のステータス
    [SerializeField, Tooltip("耐久値")] private int _maxDurability; // 最大耐久値
    [Header("属性耐性")]
    [SerializeField, Tooltip("切断耐性")] private int _slashResist;   // 切断耐性
    [SerializeField, Tooltip("衝撃耐性")] private int _crashResist;   // 衝撃耐性
    [SerializeField, Tooltip("貫通耐性")] private int _pierceResist;  // 貫通耐性
    [SerializeField, Tooltip("スコア")] private int _score;           // スコア
    [SerializeField, Tooltip("インターバル")] private float _maxDamageInterval; // インターバル
    // 参照
    private Dictionary<Type, int> _resists = new Dictionary<Type, int>();  // 属性耐性の辞書
    // private Slash _slash;   // 切断処理クラス
    private Crash _crash;     // 破壊処理クラス
    private Pierce _pierce;   // 刺突処理クラス
    // 計算用
    private int _nowDurability = 0;          // 現在の耐久値
    private float _nowDamageInterval = 0;    // インターバル値
    private bool _inDamageInterval = false;  // 現在インターバル中？

    // アクセサ
    public void SetDurability(int durability)
    {
        _nowDurability = durability;
    }

    public void SetScore(int score)
    {
        _score = score;
    }

    private void Start()
    {
        // 辞書作成
        _resists.Add(Type.slash, _slashResist);
        _resists.Add(Type.crash, _crashResist);
        _resists.Add(Type.pierce, _pierceResist);
        _resists.Add(Type.plane, 0);
        // 参照取得
        //_slash = GetComponent<Slash>();
        _crash = GetComponent<Crash>();
        _pierce = GetComponent<Pierce>();
        // パラメータ初期化
        Initialize();
    }

    private void Update()
    {
        CalcInterval();
    }

    /// <summary>
    /// パラメータ初期化
    /// </summary>
    private void Initialize()
    {
        _nowDurability = _maxDurability;
        _nowDamageInterval = 0;
        _inDamageInterval = false;
    }

    /// <summary>
    /// 連続で攻撃を受けないようにするインターバル
    /// </summary>
    private void CalcInterval()
    {
        if (_inDamageInterval)
        {
            _nowDamageInterval += Time.deltaTime;
            if (_nowDamageInterval > _maxDamageInterval)
            {
                _nowDamageInterval = 0;
                _inDamageInterval = false;
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
        // インターバル中ならスキップ
        if (_inDamageInterval) return false;
        _inDamageInterval = true;

        // ダメージ計算
        int damage = CalcDamage(receivedATK, breaker.Type);
        _nowDurability -= damage;
        Debug.Log($"nowDurability: {_nowDurability} ( -{damage} damage)");
        if (_nowDurability < 0)
        {
            Break(breaker);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 与えられた攻撃力と属性、自身の耐性、最終的なダメージの値を計算する。
    /// </summary>
    /// <param name="receivedATK">受ける攻撃力</param>
    /// <param name="attackType">受ける攻撃の属性</param>
    /// <returns></returns>
    private int CalcDamage(int receivedATK, Type attackType)
    {
        int damage = receivedATK - _resists[attackType];
        if (damage < 0) damage = 0;
        return damage;
    }

    /// <summary>
    /// 耐久値が0になり壊れるときのメソッド
    /// </summary>
    /// <param name="breaker">`攻撃した側の情報</param>
    private void Break(Breaker breaker)
    {
        Debug.Log("Break");
        int scoreRecoveryAmount = 0;
        int durabilityRecoveryAmount = 0;
        switch (breaker.Type)
        {
            case Type.slash:
                // Slashクラスを呼び出す
                _slash.CallSlash(breaker);
                break;
            case Type.crash:
                // Crashクラスを呼び出す
                _crash.CallCrash();
                break;
            case Type.pierce:
                // Pierceクラスを呼び出す
                _pierce.CallPierce(breaker, this);
                break;
            default:
                break;
        }

        ScoreController.instance.AddScore(_score);
        _nowDurability = durabilityRecoveryAmount;
        _score = scoreRecoveryAmount;

        if (_nowDurability <= 0)
        {
            Destroy(this.gameObject);
        }
    }
}
