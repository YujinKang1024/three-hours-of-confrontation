using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeIn : MonoBehaviour
{
    public Image blackPanel;
    public float fadeDuration = 1.5f;

    void OnEnable()
    {
        StartCoroutine(Fade());
    }

    IEnumerator Fade()
    {
        Color color = blackPanel.color;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float alpha = t / fadeDuration;
            blackPanel.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
        blackPanel.color = new Color(color.r, color.g, color.b, 1f);
    }
}
