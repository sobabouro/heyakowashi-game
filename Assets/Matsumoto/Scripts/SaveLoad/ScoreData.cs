using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ScoreData : MonoBehaviour
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

    public void SetuserComment(string userComment)
    {
        this.userComment = userComment;
    }

    public void SetScore(int score)
    {
        this.score = score;
    }

    public string GetUserComment()
    {
        return userComment;
    }

    public int GetScore()
    {
        return score;
    }
}
