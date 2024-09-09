using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreController : MonoBehaviour
{
    [SerializeField]
    private int score;

    // Start is called before the first frame update
    void Start()
    {
        score = 0;
        PlayerPrefs.SetInt("Score", score);
    }

    // addScore‚ðƒXƒRƒA‚É‘«‚·
    public void AddScore(int addScore)
    {
        score += addScore;
    }

    public int GetScore()
    {
        return score;
    }

    public void FinishScore()
    {
        PlayerPrefs.SetInt("Score", score);
    }
}