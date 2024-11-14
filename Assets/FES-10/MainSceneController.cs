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
    private bool _isDead = false;

    // �I�������o�ŌĂяo���C�x���g
    [SerializeField]
    private UnityEvent onFinishEvent;
    // ���U���g�ɔ�Ԃ܂ł̗P�\����
    private float _waitFOrMoveScene = 3;

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
    /// �Q�[���J�n������
    /// </summary>
    public void StartGame()
    {
        timeController.SetTimeLimit(_timeLimit);
        timeController.StartTimer();
        CollisionEvent.canEventCall = true;
    }

    /// <summary>
    /// �Q�[���I��������
    /// </summary>
    public void FinishGame()
    {
        CollisionEvent.canEventCall = false;
        scoreController.FinishScore();
        timeController.StopTimer();
        Debug.Log("FinishGame");

        if(!_isDead) onFinishEvent?.Invoke();

        StartCoroutine("MoveResult");
    }

    /// <summary>
    /// �Q�[���I�����V�[���J��
    /// </summary>
    private IEnumerator MoveResult()
    {
        Debug.Log("MoveResult");
        yield return new WaitForSeconds(_waitFOrMoveScene);
        sceneController.ChangeToTargetScene("Result");
    }

    /// <summary>
    /// ���S���t���O�ݒ�
    /// </summary>
    public void SetIsDead(bool isDead)
    {
        this._isDead = isDead;
    }
}