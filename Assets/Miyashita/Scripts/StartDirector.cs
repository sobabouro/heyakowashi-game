using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartDirector : MonoBehaviour
{
    private SceneController sceneController;

    // Start is called before the first frame update
    void Start()
    {
        // GameObjectにSceneControllerをアタッチ
        sceneController = gameObject.AddComponent<SceneController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // シーンを変更
            sceneController.ChangeToTargetScene("Title_Test2");
        }
    }
}
