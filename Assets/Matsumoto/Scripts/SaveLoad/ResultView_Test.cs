using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ResultView_Test : MonoBehaviour
{
    // プレイヤーのスコア表示用テキスト
    [SerializeField]
    private TMPro.TMP_Text playerScore;

    // ランキングのスコアを表示用テキストリスト
    [SerializeField]
    private List<TMPro.TMP_Text> rankingScoreList = new List<TMP_Text>();

    // プレイヤーのコメント用テキスト
    [SerializeField]
    private TMPro.TMP_Text playerComment; 


    // データを保持しているスクリプト
    [SerializeField]
    private CreateNewData createNewData;

    private List<ScoreData> scoreDataList;

    private ScoreData scoreData;

    void Start()
    {
        playerScore.SetText("");
        playerComment.SetText("");
        InitializeScoreData();
        ShowScore();
        scoreDataList = createNewData.GetSaveData().GetScoreDataList();        
        scoreDataList.Add(scoreData);
        SortScoreDataList();
    }

    // 現在保存しているスコアを表示する
    private void ShowScore()
    {
        playerScore.SetText(scoreData.GetScore().ToString());
        
    }

    // 現在保存しているのJSONデータを表示する
    public void ShowJsonData()
    {
        Debug.Log(createNewData.GetSaveData().GetJsonData());
    }

    // 今回のスコアデータを含めたデータをセーブする
    public void Save()
    {
        
        PlayerPrefs.SetString("PlayerData", createNewData.GetSaveData().GetJsonData());
    }

    // 今回のプレイのuserCommentを設定する
    public void SetuserComment(string userComment)
    {
        playerComment.SetText(userComment);
        scoreData.SetUserComment(userComment);
    }

    // PlayerPrefsから取得したデータをSaveDataのデータに上書きする
    public void LoadFromJsonOverwrite()
    {
        
        if (PlayerPrefs.HasKey("PlayerData"))
        {
            var data = PlayerPrefs.GetString("PlayerData");
            JsonUtility.FromJsonOverwrite(data, createNewData.GetSaveData());
            Debug.Log(createNewData.GetSaveData().GetJsonData());
        }
    }

    // 保存しているセーブデータをそのまま表示
    public void ShowSaveData()
    {
        
        if (PlayerPrefs.HasKey("PlayerData"))
        {
            var data = PlayerPrefs.GetString("PlayerData");
            SaveData otherSaveData = JsonUtility.FromJson<SaveData>(data);
            Debug.Log(otherSaveData.GetJsonData());
        }
    }

    // データを削除する
    private void DeleteData()
    {
        Debug.Log("SaveDataDeleate!");
        PlayerPrefs.DeleteKey("PlayerData");
    }

    // ScoreDataの初期化
    public void InitializeScoreData()
    {
        int score = PlayerPrefs.GetInt("Score", 0);
        string userComment = "";
        scoreData = new ScoreData(score, userComment);
    }

    // ランキングをスコアが大きい順にソートする
    public void SortScoreDataList()
    {
        var c = new Comparison<ScoreData>(Compare);
        scoreDataList.Sort(c);
    }


    static int Compare(ScoreData a, ScoreData b)
    {
        return b.GetScore() - a.GetScore();
    }

}
