using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class PlaySoundOnClick : MonoBehaviour
{
    public AudioClip clickSound; // �N���b�N���̃T�E���h�N���b�v
    private AudioSource audioSource;

    void Start()
    {
        // AudioSource �R���|�[�l���g���擾
        audioSource = GetComponent<AudioSource>();

        // �{�^���̃N���b�N�C�x���g�Ƀ��X�i�[��ǉ�
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(PlayClickSound);
        }
    }

    // �{�^�����N���b�N���ꂽ�Ƃ��ɃT�E���h���Đ����郁�\�b�h
    public void PlayClickSound()
    {
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}
