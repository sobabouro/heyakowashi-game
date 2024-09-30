using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
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

    private void Start()
    {
        enableTimer = false;
        SetTimeLimit(_defaultTimeLimit);
    }

    private void Update()
    {
        if (!enableTimer) return;
        // �J�E���g�_�E��
        _nowTime -= Time.deltaTime;
        if (_nowTime < 0) _nowTime = 0;
        // �\������
        ShowTime();

        // �J�E���g�[���ŏI�����C�x���g���Ăяo��
        if (_nowTime == 0)
        {
            enableTimer = false;
            timerFinishedEvent.Invoke();
        }
    }

    /// <summary>
    /// ���݂̎��Ԃ�\������
    /// </summary>
    private void ShowTime()
    {
        // �b��mm:ss�`���ɂ���
        int mini = (int)_nowTime / 60;
        int sec = (int)Math.Ceiling(_nowTime % 60);
        string time_str = mini + ":" + sec.ToString("00");
        // �\������
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
    /// ���݂̎��Ԃ�ύX����
    /// </summary>
    public void SetTimeLimit(float timeLimit)
    {
        _nowTime = timeLimit;
        ShowTime();
    }

    /// <summary>
    /// �J�E���g�_�E���J�n
    /// </summary>
    public void StartTimer()
    {
        enableTimer = true;
    }

    /// <summary>
    /// �J�E���g�_�E���ꎞ��~
    /// </summary>
    public void StopTimer()
    {
        enableTimer = false;
    }



}
