using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class PlaySoundOnClick : MonoBehaviour
{
    public AudioClip clickSound; // クリック時のサウンドクリップ
    private AudioSource audioSource;

    void Start()
    {
        // AudioSource コンポーネントを取得
        audioSource = GetComponent<AudioSource>();

        // ボタンのクリックイベントにリスナーを追加
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(PlayClickSound);
        }
    }

    // ボタンがクリックされたときにサウンドを再生するメソッド
    public void PlayClickSound()
    {
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}
