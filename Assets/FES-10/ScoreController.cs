using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _scoreText;
    
    private int _score;

    public static ScoreController instance;

    private void Awake()
    {
        // シングルトンの呪文
        if (instance == null)
        {
            // 自身をインスタンスとする
            instance = this;
        }
        else
        {
            // インスタンスが複数存在しないように、既に存在していたら自身を消去する
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _score = 0;
        ShowScore();
        PlayerPrefs.SetInt("Score", _score);
    }

    /// <summary>
    ///  現在のスコアを表示する
    /// </summary>
    private void ShowScore()
    {
        if (_scoreText != null)
        {
            _scoreText.SetText(_score.ToString());
        }
        else
        {
            Debug.Log($"SCORE: {_score}");
        }
    }

    /// <summary>
    ///  addScoreを現在のスコアに足す
    /// </summary>
    public void AddScore(int addScore)
    {
        _score += addScore;
        ShowScore();
        Debug.Log("AddScore : " + addScore);
    }

    public int GetScore()
    {
        return _score;
    }

    public void FinishScore()
    {
        PlayerPrefs.SetInt("Score", _score);
    }
}