using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class SaveData : object
{
    [SerializeField]
    private List<ScoreData> scoreDataList = new List<ScoreData>();

    private ScoreData scoreData;

    void start()
    {
        InitializeScoreData();
    }

    // ScoreDataの初期化
    private void InitializeScoreData()
    {
        int score = PlayerPrefs.GetInt("Score");
        string userComment = "";
        scoreData = new ScoreData(score, userComment);
    }

    public void SetScoreData(int score, string userComment)
    {
        scoreData = new ScoreData(score, userComment);
    }

    public void SetScoreDataList(List<ScoreData> scoreDataList)
    {
        this.scoreDataList = scoreDataList;
    }

    public List<ScoreData> GetScoreDataList()
    {
        return scoreDataList;
    }

    // ユーザーコメントのみを変更する
    public void SetUserComment(string userComment)
    {
        scoreData.SetuserComment(userComment);
    }

    public string GetUserComment()
    {
        return scoreData.GetUserComment();
    }

    // 現在のスコアデータを返す
    public ScoreData GetScoreData()
    {
        return scoreData;
    }

    // フィールド値をJSON形式にした文字列を返す
    public string GetJsonData()
    {
        return JsonUtility.ToJson(this);
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
