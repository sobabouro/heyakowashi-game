using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum Type { plane, slash, crash, pierce }

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
	private int score = default;

    // 属性耐性の辞書
    private Dictionary<Type, int> resists = new Dictionary<Type, int>();
    // 結合しているときの結合相手のBreakerクラス
    // private Breaker connectedMainObj = null;

    private void Start()
    {
		resists.Add(Type.slash, slashResist);
		resists.Add(Type.crash, crashResist);
		resists.Add(Type.pierce, pierceResist);
    }
}
