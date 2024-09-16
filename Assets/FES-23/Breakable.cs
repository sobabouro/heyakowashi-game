using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour
{
	[SerializeField, Tooltip("耐久値")]
	private int durability = default;
	[SerializeField, Tooltip("切断耐性"), Header("属性耐性")]
	private int slashResist = default;
	[SerializeField, Tooltip("衝撃耐性"),]
	private int crashResist = default;
	[SerializeField, Tooltip("貫通耐性"),]
	private int pierceResist = default;
	[SerializeField, Tooltip("スコア")]
	private int score = default;

}
