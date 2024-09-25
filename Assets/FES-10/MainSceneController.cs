using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class MainSceneController : MonoBehaviour
{
    [SerializeField] public ScoreController scoreController;
    [SerializeField] public TimeController timeController;
    [SerializeField] private float _timeLimit = 120;


    private void Start()
    {
        timeController.SetTimeLimit(_timeLimit);
        timeController.timerFinishedEvent.AddListener(FinishGame);
    }

    /// <summary>
    /// ゲーム開始時処理
    /// </summary>
    public void StartGame()
    {
        timeController.SetTimeLimit(_timeLimit);
        timeController.StartTimer();
    }

    /// <summary>
    /// ゲーム終了時処理
    /// </summary>
    public void FinishGame()
    {
        scoreController.FinishScore();
        Debug.Log("FinishGame");
    }

}