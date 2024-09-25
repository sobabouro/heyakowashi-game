using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.XR.OpenXR.Features.Interactions.HTCViveControllerProfile;

public class MainSceneController : MonoBehaviour
{
    [SerializeField] public ScoreController scoreController;
    [SerializeField] public TimeController timeController;
    [SerializeField] private float _timeLimit = 120;


    private void Start()
    {
        StartGame();
    }

    /// <summary>
    /// ゲーム開始時処理
    /// </summary>
    private void StartGame()
    {
        timeController.SetTimeLimit(_timeLimit);
        timeController.TimerStart();
    }

    /// <summary>
    /// ゲーム終了時処理
    /// </summary>
    private void FinishGame()
    {
        scoreController.FinishScore();
    }

}