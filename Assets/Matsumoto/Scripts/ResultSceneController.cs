using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class ResultSceneController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // TitleScene(Title_Test)‚É‘JˆÚ‚·‚é
    public void ChangeToTitleScene()
    {
        SceneManager.LoadScene("Title_Test");
    }
}
