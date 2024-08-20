using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateNewData : MonoBehaviour
{
    private SaveData saveData = new SaveData();

    public SaveData GetSaveData()
    {
        return saveData;
    }
}
