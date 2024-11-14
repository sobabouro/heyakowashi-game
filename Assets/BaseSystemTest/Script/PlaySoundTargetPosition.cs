using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundTargetPosition : MonoBehaviour
{
    [SerializeField]
    public Vector3 position;

    [SerializeField]
    public GameObject targetPositionFromObject;

    // 特定の位置でオーディオクリップを再生する
    public void PlaySoundAtPosition(AudioClip audioClip)
    {
        if (audioClip == null) return;

        if (targetPositionFromObject != null) position = targetPositionFromObject.transform.position;
        AudioSource.PlayClipAtPoint(audioClip, position);
    }

}
