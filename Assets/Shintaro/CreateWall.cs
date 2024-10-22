using System;
using System.Collections;
using System.Collections.Generic;
// using System.Data.Common.CommandTrees.ExpressionBuilder;
using UnityEngine;

[Serializable]
public class WallData
{
    public Vector3 position;
    public Vector3 size;
}

public class CreateWall : MonoBehaviour
{
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private List<WallData> wallDataList;
    // Start is called before the first frame update

    private Vector3 cubeScale;

    void Start()
    {
        cubeScale = cubePrefab.transform.localScale;

        foreach (WallData wallData in wallDataList)
        {
            Create(wallData.position, wallData.size);
        }
    }

    void Create(Vector3 pos, Vector3 size)
    {
        for (int i = 0; i < size.y; i++)
        {
            for (int j = 0; j < size.x; j++)
            {
                for (int k = 0; k < size.z; k++)
                {
                    float posX = pos.x + (cubeScale.x / 2.0f) + (j * cubeScale.x);
                    float posY = pos.y + (cubeScale.y / 2.0f) + (i * cubeScale.y);
                    float posZ = pos.z + (cubeScale.z / 2.0f) + (k * cubeScale.z);

                    GameObject cubeObj = Instantiate(cubePrefab, new Vector3(posX, posY, posZ), Quaternion.identity);
                    cubeObj.transform.parent = transform;
                }
            }
        }
    }
}
