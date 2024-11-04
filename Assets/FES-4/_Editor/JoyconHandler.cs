using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoyconHandler : MonoBehaviour
{
    private List<Joycon> joycons;

    [SerializeField]
    public int jc_ind = 0;

    // Values made available via Unity
    public float[] stick;
    public Quaternion orientation;

    void Start()
    {
        // get the public Joycon array attached to the JoyconManager in scene
        joycons = JoyconManager.Instance.j;
        if (joycons.Count < jc_ind + 1)
        {
            Destroy(gameObject);
        }

        if (jc_ind == 0)
        {
            gameObject.GetComponent<Renderer>().material.color = Color.blue;
        }
        else if (jc_ind == 1)
        {
            gameObject.GetComponent<Renderer>().material.color = Color.red;
        }
        else if (jc_ind == 2)
        {
            gameObject.GetComponent<Renderer>().material.color = Color.green;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // make sure the Joycon only gets checked if attached
        if (joycons.Count > 0)
        {
            Joycon j = joycons[jc_ind];

            if (j.GetButton(Joycon.Button.SHOULDER_2))
            {
                foreach (Joycon joycon in joycons)
                {
                    joycon.Recenter();
                }
            }

            if (j.GetButtonDown(Joycon.Button.DPAD_DOWN))
            {
                Debug.Log("Rumble");

                // Rumble for 200 milliseconds, with low frequency rumble at 160 Hz and high frequency rumble at 320 Hz. For more information check:
                // https://github.com/dekuNukem/Nintendo_Switch_Reverse_Engineering/blob/master/rumble_data_table.md

                j.SetRumble(160, 320, 0.6f, 200);

                // The last argument (time) in SetRumble is optional. Call it with three arguments to turn it on without telling it when to turn off.
                // (Useful for dynamically changing rumble values.)
                // Then call SetRumble(0,0,0) when you want to turn it off.
            }

            // stick = j.GetStick();

            // ‰ñ“]
            gameObject.transform.rotation = j.GetVector();
        }
    }

}