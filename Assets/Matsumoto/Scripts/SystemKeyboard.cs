using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MixedReality.Toolkit.Input;
using MixedReality.Toolkit.UX;

public class SystemKeyboard : MonoBehaviour
{
    [SerializeField]
    private TouchScreenKeyboard keyboard;

    [SerializeField]
    private ResultView_Test resultView_Test;

    [SerializeField]
    private TextMeshPro debugMessage = null;

    [SerializeField]
    private KeyboardPreview mixedRealityKeyboardPreview = null;

    private string keyboardText;

    void Start()
    {
        // Initially hide the preview.
        if (mixedRealityKeyboardPreview != null)
        {
            mixedRealityKeyboardPreview.gameObject.SetActive(false);
        }
        keyboardText = "";
    }

    // キーボードを呼び出す
    public void OpenSystemKeyboard()
    {
        if (keyboard == null)
        {
            keyboard = TouchScreenKeyboard.Open(keyboardText, TouchScreenKeyboardType.Default);
        }
        else
        {
            keyboard.active = true;
        }
        Debug.Log("OpenSystemKeyboard");
    }

    // キーボードをしまう
    public void CloseSystemKeyboard()
    {
        if(keyboard != null)
        {
            keyboard.active = false;
        }
    }

    // キーボードで打ったテキストをユーザーコメントに代入する
    public void SetComment()
    {
        resultView_Test.SetUserComment(keyboardText);
    }

    private void Update()
    {
        if (keyboard != null)
        {
            keyboardText = keyboard.text;
            // Do stuff with keyboardText

            if (TouchScreenKeyboard.visible)
            {
                if (debugMessage != null)
                {
                    debugMessage.text = "typing... : " + keyboardText;
                }
            }
            else
            {
                if (debugMessage != null)
                {
                    debugMessage.text = "typed : " + keyboardText;
                }

                keyboard = null;
            }
        }    
    }
}
