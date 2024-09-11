using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _scoreText;
    
    private int _score;

    // Start is called before the first frame update
    void Start()
    {
        _score = 0;
        if (_scoreText != null) {
            _scoreText.SetText($"SCORE: {_score}");
        }
        else {
            Debug.Log($"SCORE: {_score}");
        }
        PlayerPrefs.SetInt("Score", _score);
    }

    // addScore‚ðƒXƒRƒA‚É‘«‚·
    public void AddScore(int addScore)
    {
        _score += addScore;

        if (_scoreText != null) {
            _scoreText.SetText($"SCORE: {_score} (+ {addScore})");
        }
        else {
            Debug.Log($"SCORE: {_score} (+ {addScore})");
        }
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