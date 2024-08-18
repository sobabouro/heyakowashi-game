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
    private TextMeshPro debugMessage = null;

    [SerializeField]
    private KeyboardPreview mixedRealityKeyboardPreview = null;

    void Start()
    {
        // Initially hide the preview.
        if (mixedRealityKeyboardPreview != null)
        {
            mixedRealityKeyboardPreview.gameObject.SetActive(false);
        }
    }

    public void OpenSystemKeyboard()
    {
        keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false, false, false, false);
        Debug.Log("OpenSystemKeyboard");
    }

    private void Update()
    {
        if (keyboard != null)
        {
            string keyboardText = keyboard.text;
            // Do stuff with keyboardText

            if (TouchScreenKeyboard.visible)
            {
                if (debugMessage != null)
                {
                    debugMessage.text = "typing... " + keyboardText;
                }
            }
            else
            {
                if (debugMessage != null)
                {
                    debugMessage.text = "typed " + keyboardText;
                }

                keyboard = null;
            }
        }
    }
}
