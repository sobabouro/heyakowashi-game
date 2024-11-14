using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartDirector : MonoBehaviour
{
    private SceneController sceneController;

    // Start is called before the first frame update
    void Start()
    {
        // GameObject��SceneController���A�^�b�`
        sceneController = gameObject.AddComponent<SceneController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // �V�[����ύX
            sceneController.ChangeToTargetScene("Main");
        }
    }
}
