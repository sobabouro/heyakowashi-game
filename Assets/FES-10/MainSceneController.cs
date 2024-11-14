using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class MainSceneController : MonoBehaviour
{
    private ScoreController scoreController;
    private TimeController timeController;
    private SceneController sceneController;
    [SerializeField] private float _timeLimit = 120;

    public static MainSceneController instance;
    public bool _isDead = false;

    // 終了時演出で呼び出すイベント
    [SerializeField]
    private UnityEvent onFinishEvent;
    // リザルトに飛ぶまでの猶予時間
    private float _waitFOrMoveScene = 3;

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


    private void Start()
    {
        scoreController = ScoreController.instance;
        timeController = TimeController.instance;
        sceneController = SceneController.instance;

        CollisionEvent.canEventCall = false;

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
        CollisionEvent.canEventCall = true;
    }

    /// <summary>
    /// ゲーム終了時処理
    /// </summary>
    public void FinishGame()
    {
        CollisionEvent.canEventCall = false;
        scoreController.FinishScore();
        Debug.Log("FinishGame");

        if(!_isDead) onFinishEvent?.Invoke();

        StartCoroutine("MoveResult");
    }

    /// <summary>
    /// ゲーム終了時演出
    /// </summary>
    private IEnumerator MoveResult()
    {
        Debug.Log("MoveResult");
        yield return new WaitForSeconds(_waitFOrMoveScene);
        sceneController.ChangeToTargetScene("Result");
    }
}