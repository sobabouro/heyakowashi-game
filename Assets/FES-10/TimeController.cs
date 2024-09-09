using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    [SerializeField]
    private int _startTime = 0;

    private float _nowTime;

    public float NowTime
    {
        get => _nowTime;
        set => _nowTime = value;
    }

    private void Start()
    {
        _nowTime = _startTime;
    }

    private void Update()
    {
        _nowTime -= Time.deltaTime;
        if (_nowTime < 0) { _nowTime = 0; }
    }

}
