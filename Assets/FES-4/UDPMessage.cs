using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


public struct Message
{
    public byte[] bytes;
    public System.DateTime time;

    public Message(byte[] b, System.DateTime t)
    {
        bytes = b;
        time = t;
    }


    public override string ToString()
    {
        string temp = Encoding.UTF8.GetString(bytes);
        return temp;
    }

}