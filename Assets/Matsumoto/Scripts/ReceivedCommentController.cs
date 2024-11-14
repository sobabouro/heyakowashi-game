using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ReceivedCommentController : MonoBehaviour
{
    [SerializeField]
    private GameObject commentPrefab;

    // �R�����g��z�u����͈͂����߂�Ίp��A�_
    [SerializeField]
    private Transform diagonalA;

    [SerializeField]
    private Transform diagonalB;

    [SerializeField]
    private GameObject player;

    [SerializeField]
    private GameObject container;

    public void CreateReceivedComment(List<ScoreData> scoreDataList)
    {
        if (scoreDataList.Count == 0) return;
        float x, y, z = 0;
        foreach (ScoreData scoreData in scoreDataList)
        {
            x = Random.Range(diagonalA.position.x, diagonalB.position.x);
            y = Random.Range(diagonalA.position.y, diagonalB.position.y);
            z = Random.Range(diagonalA.position.z, diagonalB.position.z);
            GameObject obj = Instantiate(commentPrefab, new Vector3(x, y, z), Quaternion.identity, container.transform);
            Debug.Log("Object����");
            obj.transform.LookAt(player.transform);
            obj.transform.Rotate(0, 180, 0);
            obj.GetComponent<TMP_Text>().SetText(scoreData.GetUserComment());
        }
    }
}
