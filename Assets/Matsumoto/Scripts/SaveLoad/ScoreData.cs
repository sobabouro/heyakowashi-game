using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ScoreData : object
{
    [SerializeField]
    private int score;
    [SerializeField]
    private string userComment;

    public ScoreData(int score, string userComment)
    {
        this.score = score;
        this.userComment = userComment;
    }

    public void SetUserComment(string userComment)
    {
        this.userComment = userComment;
    }

    public void SetScore(int score)
    {
        this.score = score;
    }

    public string GetUserComment()
    {
        if (userComment == null) return "";
        return userComment;
    }

    public int GetScore()
    {
        return score;
    }
}
