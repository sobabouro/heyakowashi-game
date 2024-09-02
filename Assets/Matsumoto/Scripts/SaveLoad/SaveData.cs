using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class SaveData : object
{
    [SerializeField]
    private List<ScoreData> scoreDataList = new List<ScoreData>();

    public void SetScoreDataList(List<ScoreData> scoreDataList)
    {
        this.scoreDataList = scoreDataList;
    }

    public List<ScoreData> GetScoreDataList()
    {
        return scoreDataList;
    }

    // フィールド値をJSON形式にした文字列を返す
    public string GetJsonData()
    {
        return JsonUtility.ToJson(this);
    }
}
