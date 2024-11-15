using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class tutorial_TakeWeapon : MonoBehaviour
{
    [SerializeField, Tooltip("表示するUIテキスト")]
    private TMP_Text messageText; // TextMeshPro用のフィールド

    [SerializeField, Tooltip("EquipWeapons スクリプトがアタッチされたオブジェクト")]
    private EquipWeapons equipWeapons; // EquipWeapons スクリプトへの参照

    [SerializeField, Tooltip("持ったときのメッセージ")]
    private string holdingMessage = "You are holding a weapon!";

    [SerializeField, Tooltip("通常時のメッセージ")]
    private string defaultMessage = "No weapon equipped.";

    private static bool isGameStart = false;

    /// <summary>
    /// ゲーム開始時にデフォルトのメッセージを設定
    /// </summary>
    private void Start()
    {
        if (messageText != null)
        {
            messageText.text = defaultMessage;
        }
    }

    /// <summary>
    /// 毎フレーム武器の装備状態をチェック
    /// </summary>
    private void Update()
    {
        if (isGameStart) return;

        if (equipWeapons != null && messageText != null)
        {
            // 武器の装備状態を確認
            if (equipWeapons.GetIsEquipWeapon())
            {
                messageText.text = holdingMessage;
            }
            else
            {
                messageText.text = defaultMessage;
            }
        }
        else if (equipWeapons == null)
        {
            Debug.LogWarning("EquipWeapons script reference is not set in the inspector.");
        }
    }

    public void SetIsGameStart()
    {
        isGameStart = true;
    }
}
