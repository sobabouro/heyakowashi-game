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
        // �V���O���g���̎���
        if (instance == null)
        {
            // ���g���C���X�^���X�Ƃ���
            instance = this;
        }
        else
        {
            // �C���X�^���X���������݂��Ȃ��悤�ɁA���ɑ��݂��Ă����玩�g����������
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
    ///  ���݂̃X�R�A��\������
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
    ///  addScore�����݂̃X�R�A�ɑ���
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