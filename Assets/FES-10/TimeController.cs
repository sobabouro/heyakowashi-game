using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

public class TimeController : MonoBehaviour
{ 
    [SerializeField] private float _defaultTimeLimit = 120;
    [SerializeField] private TextMeshProUGUI timeText = null;

    [SerializeField] public UnityEvent timerFinishedEvent = null;

    private float _nowTime;

    public float NowTime
    {
        get => _nowTime;
    }

    private bool enableTimer = false;

    private void Awake()
    {
        enableTimer = false;
        SetTimeLimit(_defaultTimeLimit);
    }

    private void Update()
    {
        if (!enableTimer) return;
        // カウントダウン
        _nowTime -= Time.deltaTime;
        if (_nowTime < 0) _nowTime = 0;
        // 表示する
        ShowTime();

        // カウントゼロで終了時イベントを呼び出し
        if (_nowTime == 0)
        {
            enableTimer = false;
            timerFinishedEvent.Invoke();
        }
    }

    /// <summary>
    /// 現在の時間を表示する
    /// </summary>
    private void ShowTime()
    {
        // 秒をmm:ss形式にする
        int mini = (int)_nowTime / 60;
        int sec = (int)Math.Ceiling(_nowTime % 60);
        string time_str = mini + ":" + sec.ToString("00");
        // 表示する
        if (timeText != null)
        {
            timeText.SetText(time_str);
        }
        else
        {
            Debug.Log(time_str);
        }
    }

    /// <summary>
    /// 現在の時間を変更する
    /// </summary>
    public void SetTimeLimit(float timeLimit)
    {
        _nowTime = timeLimit;
        ShowTime();
    }

    /// <summary>
    /// カウントダウン開始
    /// </summary>
    public void StartTimer()
    {
        enableTimer = true;
    }

    /// <summary>
    /// カウントダウン一時停止
    /// </summary>
    public void StopTimer()
    {
        enableTimer = false;
    }



}
