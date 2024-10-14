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

    // 今までのコメントを表示するコンポーネント
    [SerializeField]
    private ReceivedCommentController receivedCommentController;

    // データを保持しているスクリプト
    [SerializeField]
    private CreateNewData createNewData;

    private List<ScoreData> scoreDataList;

    private ScoreData scoreData;

    private int playerRankingIndex;

    [SerializeField]
    private bool deleateDataFlug = false;

    void Start()
    {
        if(deleateDataFlug)  DeleteData();
        playerScore.SetText("");
        playerComment.SetText("");
        InitializeScoreData();
        ShowScore();
        scoreDataList = createNewData.GetSaveData().GetScoreDataList();
        receivedCommentController.CreateReceivedComment(scoreDataList);
        scoreDataList.Add(scoreData);
        ShowRanking3Score();
        playerRankingIndex = GetPlayerRanking();
        Save();
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
        createNewData.GetSaveData().SetScoreDataList(scoreDataList);
        PlayerPrefs.SetString("PlayerData", createNewData.GetSaveData().GetJsonData());
        Debug.Log("JsonData: " + createNewData.GetSaveData().GetJsonData());
        Debug.Log("PlayerPrefsData: " + PlayerPrefs.GetString("PlayerData")) ;
    }

    // 今回のプレイのuserCommentを設定する
    public void SetUserComment(string userComment)
    {
        playerComment.SetText(userComment);
        scoreData.SetUserComment(userComment);
        scoreDataList[playerRankingIndex] = scoreData;
        Save();
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

    // プレイヤーのランキング-1を返す
    private int GetPlayerRanking()
    {
        return scoreDataList.IndexOf(scoreData);
    }

    // ランキングのトップ3のスコアを表示する
    private void ShowRanking3Score()
    {
        SortScoreDataList();
        Debug.Log(scoreDataList[0].GetScore());
        for (int i = 0; i < 3; i++)
        {
            Debug.Log("Ranking" + i+1 + " : " + scoreDataList[i].GetScore());
            rankingScoreList[i].SetText(scoreDataList[i].GetScore().ToString());
        }
    }


    static int Compare(ScoreData a, ScoreData b)
    {
        return b.GetScore() - a.GetScore();
    }

}
