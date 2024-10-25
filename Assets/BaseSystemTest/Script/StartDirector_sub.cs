using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartDirector_sub : MonoBehaviour
{
    private SceneController sceneController;

    // Start is called before the first frame update
    void Start()
    {
        sceneController = SceneController.instance;
    }

    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetMouseButtonDown(0))
        {
            // ÉVÅ[ÉìÇïœçX
            sceneController.ChangeToTargetScene("Main");
        }*/
    }

    public void SceneChange()
    {
        sceneController.ChangeToTargetScene("Main");
    }
}
