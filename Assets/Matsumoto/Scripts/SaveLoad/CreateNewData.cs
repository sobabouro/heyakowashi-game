using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateNewData : MonoBehaviour
{
    private SaveData saveData = new SaveData();
    [SerializeField]
    public bool debugFlug;
    void Awake()
    {
        if (debugFlug)
        {
            Debug.Log("Debug�J�n!");
            // Debug�p�f�[�^
            CreateDebugSaveData();
        }
        else
        {
            // ���K���s�p�f�[�^
            CreateSaveData();
        }
    }

    public SaveData GetSaveData()
    {
        return saveData;
    }

    // �f�o�b�O�p�̃f�[�^�̍쐬
    private void CreateDebugSaveData()
    {
        ScoreData scoreData1 = new ScoreData(1000, "Test1");
        ScoreData scoreData2 = new ScoreData(2000, "Test2");
        ScoreData scoreData3 = new ScoreData(4000, "Test3");
        ScoreData scoreData4 = new ScoreData(3000, "Test4");
        List<ScoreData> debugScoreDataList = new List<ScoreData>();
        debugScoreDataList.Add(scoreData1);
        debugScoreDataList.Add(scoreData2);
        debugScoreDataList.Add(scoreData3);
        debugScoreDataList.Add(scoreData4);
        saveData.SetScoreDataList(debugScoreDataList);
    }

    // PlayerPrefs����擾�����f�[�^��SaveData�̃f�[�^�ɏ㏑������
    private void CreateSaveData()
    {
        if (PlayerPrefs.HasKey("PlayerData"))
        {
            var data = PlayerPrefs.GetString("PlayerData");
            JsonUtility.FromJsonOverwrite(data, saveData);
            Debug.Log(saveData.GetJsonData());
        }
        else
        {
            ScoreData scoreData_empty = new ScoreData(0, "");
            List<ScoreData> scoreDataList = new List<ScoreData>();
            scoreDataList.Add(scoreData_empty);
            saveData.SetScoreDataList(scoreDataList);
        }

    }
}
