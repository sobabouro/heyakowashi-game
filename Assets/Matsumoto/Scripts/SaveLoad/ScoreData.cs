using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ScoreData : MonoBehaviour
{
    [SerializeField]
    private int score;
    [SerializeField]
    private string userName;

    public ScoreData(int score, string userName)
    {
        this.score = score;
        this.userName = userName;
    }

    public void SetUserName(string userName)
    {
        this.userName = userName;
    }

    public void SetScore(int score)
    {
        this.score = score;
    }

    public string GetUserName()
    {
        return userName;
    }

    public string GetScore()
    {
        return userName;
    }
}
