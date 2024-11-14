using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneDirector : MonoBehaviour
{
    public Image fadeImage; // �t�F�[�h�p�̍���Image
    public float fadeDuration = 1.0f; // �t�F�[�h�̎�������

    // �V�[�����������Ƃ��Ď󂯎�郁�\�b�h
    public void LoadSceneWithFade(string sceneName)
    {
        Debug.Log("Starting transition to scene: " + sceneName);
        StartCoroutine(FadeOutAndLoadScene(sceneName));
    }

    private IEnumerator FadeOutAndLoadScene(string sceneName)
    {
        // �t�F�[�h�A�E�g
        float elapsedTime = 0.0f;
        Color color = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        // �V�[���J��
        SceneManager.LoadScene(sceneName);
    }
}
