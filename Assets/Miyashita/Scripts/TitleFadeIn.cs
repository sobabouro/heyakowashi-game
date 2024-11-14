using UnityEngine;
using UnityEngine.UI;

public class TitleFadeIn : MonoBehaviour
{
    public Text titleText;  // �^�C�g����Text�R���|�[�l���g��Inspector�Őݒ�
    public float fadeDuration = 2.0f;  // �t�F�[�h�C���̎���
    private Color originalColor;  // ���̐F��ێ�
    private float timer = 0.0f;  // �^�C�}�[

    void Start()
    {

        // ���̃J���[���擾���A�A���t�@�l��0�ɐݒ�
        originalColor = titleText.color;

        Color transparentColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0);
        titleText.color = transparentColor;
    }

    void Update()
    {
        // �t�F�[�h�C������
        if (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            titleText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            // �f�o�b�O�Ń^�C�}�[�ƃA���t�@�l��\��
            Debug.Log("Timer: " + timer + " / Alpha Value: " + alpha);
        }
    }
}
