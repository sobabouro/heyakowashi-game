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
    public Message(byte type, Quaternion q, System.DateTime t)
    {
        bytes = new byte[1 + sizeof(float) * 4];
        bytes[0] = type;
        Array.Copy(BitConverter.GetBytes(q.x), 0, bytes, 1 + 0 * sizeof(float), sizeof(float));
        Array.Copy(BitConverter.GetBytes(q.y), 0, bytes, 1 + 1 * sizeof(float), sizeof(float));
        Array.Copy(BitConverter.GetBytes(q.z), 0, bytes, 1 + 2 * sizeof(float), sizeof(float));
        Array.Copy(BitConverter.GetBytes(q.w), 0, bytes, 1 + 3 * sizeof(float), sizeof(float));
        time = t;
    }

    public Message(byte type, bool[] boolean_data, System.DateTime t)
    {
        bytes = new byte[1 + sizeof(bool) * boolean_data.Length];
        bytes[0] = type;

        for (int index = 0; index < boolean_data.Length; index++)
        {
            Array.Copy(BitConverter.GetBytes(boolean_data[index]), 0, bytes, 1 + index * sizeof(bool), sizeof(bool));
        }
        time = t;
    }


    public override string ToString()
    {
        string temp = Encoding.UTF8.GetString(bytes);
        return temp;
    }

    public Quaternion ToQuaternion()
    {
        Quaternion q = Quaternion.identity;
        q.x = BitConverter.ToSingle(bytes, 1 + 0 * sizeof(float));
        q.y = BitConverter.ToSingle(bytes, 1 + 1 * sizeof(float));
        q.z = BitConverter.ToSingle(bytes, 1 + 2 * sizeof(float));
        q.w = BitConverter.ToSingle(bytes, 1 + 3 * sizeof(float));
        return q;
    }
}