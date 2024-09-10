using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainSceneController : MonoBehaviour
{
    [SerializeField] public ScoreController scoreController;
    [SerializeField] private int _startTime = 0;

    private float _nowTime;

    public float NowTime
    {
        get => _nowTime;
        set => _nowTime = value;
    }

    private bool enableCountdown;

    private void Start()
    {
        _nowTime = _startTime;
        ScoreController.Init();
    }

    private void Update()
    {
        if (!enableCountdown) return;
        // カウントダウン
        _nowTime -= Time.deltaTime;
        if (_nowTime < 0) { _nowTime = 0; }

    }

    /// <summary>
    /// ゲーム開始時処理
    /// </summary>
    private void StartGame()
    {
        enableCountdown = true;
    }

    /// <summary>
    /// ゲーム終了時処理
    /// </summary>
    private void FinishGame()
    {
        enableCountdown = false;
    }
}
