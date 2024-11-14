using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneDirector : MonoBehaviour
{
    public Image fadeImage; // フェード用の黒いImage
    public float fadeDuration = 1.0f; // フェードの持続時間

    // シーン名を引数として受け取るメソッド
    public void LoadSceneWithFade(string sceneName)
    {
        Debug.Log("Starting transition to scene: " + sceneName);
        StartCoroutine(FadeOutAndLoadScene(sceneName));
    }

    private IEnumerator FadeOutAndLoadScene(string sceneName)
    {
        // フェードアウト
        float elapsedTime = 0.0f;
        Color color = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        // シーン遷移
        SceneManager.LoadScene(sceneName);
    }
}
