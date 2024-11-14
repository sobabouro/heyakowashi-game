using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class SaveData : object
{
    [SerializeField]
    private List<ScoreData> scoreDataList;

    public void SetScoreDataList(List<ScoreData> scoreDataList)
    {
        this.scoreDataList = scoreDataList;
    }

    public List<ScoreData> GetScoreDataList()
    {
        return scoreDataList;
    }

    // �t�B�[���h�l��JSON�`���ɂ����������Ԃ�
    public string GetJsonData()
    {
        return JsonUtility.ToJson(this);
    }
}
