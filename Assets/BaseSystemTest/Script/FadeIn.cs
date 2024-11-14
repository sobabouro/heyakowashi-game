using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FadeIn : MonoBehaviour
{
    [SerializeField]
    private TMP_Text fadeInText;
    [SerializeField]
    private float fadeInSecond = 1.0f;
    [SerializeField]
    private float targetAlphaValue = 1.0f;
    private Color originalColor;
    private float timer = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        originalColor = fadeInText.color;
        Color transparentColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0);
        fadeInText.color = transparentColor;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowText()
    {
        StartCoroutine(Fade(targetAlphaValue));
    }

    private IEnumerator Fade(float targetAlphaValue)
    {
        while (timer <= fadeInSecond)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0, targetAlphaValue, timer / fadeInSecond);
            fadeInText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            yield return null;
        }
    }
}
