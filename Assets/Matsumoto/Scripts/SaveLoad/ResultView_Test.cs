using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultView_Test : MonoBehaviour
{
    // データ表示用テキスト
    [SerializeField]
    private TMPro.TMP_Text dataText;

    // データを保持しているスクリプト
    [SerializeField]
    private CreateNewData createNewData;

    // データ表示のテキストを空にする
    public void ResetText()
    {
        dataText.SetText("");
    }

    // 現在保存しているユーザー名とスコアを表示する
    public void ShowScoreData()
    {
        ResetText();
        dataText.SetText(createNewData.GetSaveData().GetScoreData());
    }

    // 現在保存しているのJSONデータを表示する
    public void ShowJsonData()
    {
        ResetText();
        dataText.SetText(createNewData.GetSaveData().GetJsonData());
    }

    // 今回のスコアデータを含めたデータをセーブする
    public void Save()
    {
        ResetText();
        PlayerPrefs.SetString("PlayerData", createNewData.GetSaveData().GetJsonData());
    }

    // 今回のプレイのuserNameを設定する
    public void SetUserName(string userName)
    {
        createNewData.GetSaveData().SetUserName(userName);
    }

    // PlayerPrefsから取得したデータをSaveDataのデータに上書きする
    public void LoadFromJsonOverwrite()
    {
        ResetText();
        if (PlayerPrefs.HasKey("PlayerData"))
        {
            var data = PlayerPrefs.GetString("PlayerData");
            JsonUtility.FromJsonOverwrite(data, createNewData.GetSaveData());
            dataText.SetText(createNewData.GetSaveData().GetJsonData());
        }
    }

    // 保存しているセーブデータをそのまま表示
    public void ShowSaveData()
    {
        ResetText();
        if (PlayerPrefs.HasKey("PlayerData"))
        {
            var data = PlayerPrefs.GetString("PlayerData");
            SaveData otherSaveData = JsonUtility.FromJson<SaveData>(data);
            dataText.SetText(otherSaveData.GetJsonData());
        }
    }

    // データを削除する
    private void DeleteData()
    {
        ResetText();
        PlayerPrefs.DeleteKey("PlayerData");
    }
}
