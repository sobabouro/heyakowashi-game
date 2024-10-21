using UnityEngine;
using UnityEngine.UI;

public class TitleFadeIn : MonoBehaviour
{
    public Text titleText;  // タイトルのTextコンポーネントをInspectorで設定
    public float fadeDuration = 2.0f;  // フェードインの時間
    private Color originalColor;  // 元の色を保持
    private float timer = 0.0f;  // タイマー

    void Start()
    {

        // 元のカラーを取得し、アルファ値を0に設定
        originalColor = titleText.color;

        Color transparentColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0);
        titleText.color = transparentColor;
    }

    void Update()
    {
        // フェードイン処理
        if (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            titleText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            // デバッグでタイマーとアルファ値を表示
            Debug.Log("Timer: " + timer + " / Alpha Value: " + alpha);
        }
    }
}
