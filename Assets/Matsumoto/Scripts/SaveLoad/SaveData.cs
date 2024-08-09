using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData : object
{
    [SerializeField]
    private List<ScoreData> scoreDataList = new List<ScoreData>();

    private ScoreData scoreData;

    public void SetScoreData(int score, string userName)
    {
        scoreData = new ScoreData(score, userName);
    }

    public void SetScoreDataList(List<ScoreData> scoreDataList)
    {
        this.scoreDataList = scoreDataList;
    }

    public List<ScoreData> GetScoreDataList()
    {
        return scoreDataList;
    }

    // ユーザー名のみを変更する
    public void SetUserName(string userName)
    {
        scoreData.SetUserName(userName);
    }

    // 全てのスコアデータのユーザー名とスコアを返す
    public string GetScoreData()
    {
        string objectString = "";
        foreach (ScoreData scoreData in scoreDataList)
        {
            objectString += "UserName: " + scoreData.GetUserName() + " Score" + scoreData.GetScore() + "\n";
        }

        return objectString;
    }

    // フィールド値をJSOn形式にした文字列を返す
    public string GetJsonData()
    {
        return JsonUtility.ToJson(this);
    }

}
