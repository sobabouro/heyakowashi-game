using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ResultView_Test : MonoBehaviour
{
    // �v���C���[�̃X�R�A�\���p�e�L�X�g
    [SerializeField]
    private TMPro.TMP_Text playerScore;

    // �����L���O�̃X�R�A��\���p�e�L�X�g���X�g
    [SerializeField]
    private List<TMPro.TMP_Text> rankingScoreList = new List<TMP_Text>();

    // �v���C���[�̃R�����g�p�e�L�X�g
    [SerializeField]
    private TMPro.TMP_Text playerComment;

    // ���܂ł̃R�����g��\������R���|�[�l���g
    [SerializeField]
    private ReceivedCommentController receivedCommentController;

    // �f�[�^��ێ����Ă���X�N���v�g
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

    // ���ݕۑ����Ă���X�R�A��\������
    private void ShowScore()
    {
        playerScore.SetText(scoreData.GetScore().ToString());
        
    }

    // ���ݕۑ����Ă����JSON�f�[�^��\������
    public void ShowJsonData()
    {
        Debug.Log(createNewData.GetSaveData().GetJsonData());
    }

    // ����̃X�R�A�f�[�^���܂߂��f�[�^���Z�[�u����
    public void Save()
    {
        createNewData.GetSaveData().SetScoreDataList(scoreDataList);
        PlayerPrefs.SetString("PlayerData", createNewData.GetSaveData().GetJsonData());
        Debug.Log("JsonData: " + createNewData.GetSaveData().GetJsonData());
        Debug.Log("PlayerPrefsData: " + PlayerPrefs.GetString("PlayerData")) ;
    }

    // ����̃v���C��userComment��ݒ肷��
    public void SetUserComment(string userComment)
    {
        playerComment.SetText(userComment);
        scoreData.SetUserComment(userComment);
        scoreDataList[playerRankingIndex] = scoreData;
        Save();
    }

    // �ۑ����Ă���Z�[�u�f�[�^�����̂܂ܕ\��
    public void ShowSaveData()
    {
        
        if (PlayerPrefs.HasKey("PlayerData"))
        {
            var data = PlayerPrefs.GetString("PlayerData");
            SaveData otherSaveData = JsonUtility.FromJson<SaveData>(data);
            Debug.Log(otherSaveData.GetJsonData());
        }
    }

    // �f�[�^���폜����
    private void DeleteData()
    {
        Debug.Log("SaveDataDeleate!");
        PlayerPrefs.DeleteKey("PlayerData");
    }

    // ScoreData�̏�����
    public void InitializeScoreData()
    {
        int score = PlayerPrefs.GetInt("Score", 0);
        string userComment = "";
        scoreData = new ScoreData(score, userComment);
    }

    // �����L���O���X�R�A���傫�����Ƀ\�[�g����
    public void SortScoreDataList()
    {
        var c = new Comparison<ScoreData>(Compare);
        scoreDataList.Sort(c);
    }

    // �v���C���[�̃����L���O-1��Ԃ�
    private int GetPlayerRanking()
    {
        return scoreDataList.IndexOf(scoreData);
    }

    // �����L���O�̃g�b�v3�̃X�R�A��\������
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
