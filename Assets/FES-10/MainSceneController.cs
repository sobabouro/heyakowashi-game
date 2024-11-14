using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainSceneController : MonoBehaviour
{
    private ScoreController scoreController;
    private TimeController timeController;
    private SceneController sceneController;
    [SerializeField] private float _timeLimit = 120;

    public static MainSceneController instance;

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
        scoreController.FinishScore();
        Debug.Log("FinishGame");

        sceneController.ChangeToTargetScene("Result");
    }

}