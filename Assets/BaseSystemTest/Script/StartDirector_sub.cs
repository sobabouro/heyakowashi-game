using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartDirector_sub : MonoBehaviour
{
    private SceneController sceneController = SceneController.instance;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetMouseButtonDown(0))
        {
            // シーンを変更
            sceneController.ChangeToTargetScene("Main");
        }*/
    }

    public void SceneChange()
    {
        sceneController.ChangeToTargetScene("Main");
    }
}
