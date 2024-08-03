using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class TitleSceneController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // MainScene(Main_Test)‚É‘JˆÚ‚·‚é
    public void ChangeToMainScene()
    {
        SceneManager.LoadScene("Main_Test");
    }
}
