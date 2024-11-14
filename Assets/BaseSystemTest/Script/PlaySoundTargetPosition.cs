using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundTargetPosition : MonoBehaviour
{
    [SerializeField]
    public Vector3 position;

    [SerializeField]
    public GameObject targetPositionFromObject;

    // ����̈ʒu�ŃI�[�f�B�I�N���b�v���Đ�����
    public void PlaySoundAtPosition(AudioClip audioClip)
    {
        if (audioClip == null) return;

        if (targetPositionFromObject != null) position = targetPositionFromObject.transform.position;
        AudioSource.PlayClipAtPoint(audioClip, position);
    }

}
