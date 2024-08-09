using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateNewData : MonoBehaviour
{
    private SaveData saveData;

    // Start is called before the first frame update
    void Start()
    {
        saveData = new SaveData();
    }

    public SaveData GetSaveData()
    {
        return saveData;
    }
}
