using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MainSceneController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 後々別のコンポーネントに機能が移動する可能性あり
    // ResultScene(Result_Test)に遷移する
    public void ChangeToResultScene()
    {
        SceneManager.LoadScene("Result_Test");
    }
}
