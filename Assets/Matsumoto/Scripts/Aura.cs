using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;

public class Aura : MonoBehaviour
{
    // 自害するときに呼び出すイベント
    [SerializeField]
    private UnityEvent onAzeluzeEvent;
    [SerializeField]
    private int waitForSceneMoveSecond = 3000;

    private MainSceneController mainSceneController;

    // Start is called before the first frame update
    void Start()
    {
        mainSceneController = MainSceneController.instance;

        this.gameObject.GetComponent<Pierce>().onBreakEvent.AddListener(Azeluze);
        this.gameObject.GetComponent<Crash>().onBreakEvent.AddListener(Azeluze);
        // this.gameObject.GetComponent<ActSubdivide>().onBreakEvent.AddListener(Azeluze);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Azeluze()
    {
        Debug.Log("あ、ありえない…");
        onAzeluzeEvent?.Invoke();

        MoveResult();
    }

    private async void MoveResult()
    {
        await Task.Delay(waitForSceneMoveSecond);
        MainSceneController.instance.FinishGame();
    }
}
