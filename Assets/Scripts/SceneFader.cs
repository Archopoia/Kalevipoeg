using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneFader : MonoBehaviour
{
   public Image img;
    public bool fadeOut = false;
    public AudioClip musicClip;


    private void Start()
    {
        if (fadeOut)
            StartCoroutine(FadeOut());
        else
            FadeAndLoad("MainScene", 1);
    }
    public void FadeAndLoad(string sceneName, float duration)
    {
        StartCoroutine(Fader(sceneName, duration));
    }

    IEnumerator Fader(string sceneName, float duration)
    {
        float t = 0;
        Color c = img.color;
        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = t / duration;
            img.color = c;
            yield return null;
        }

        SceneManager.LoadScene(sceneName);

    }

    IEnumerator FadeOut()
    {
        float t = 0;
        Color c = img.color;
        while (t < 1)
        {
            t += Time.deltaTime;
            c.a = 1f - (t / 1f);
            img.color = c;
            yield return null;
        }
    }
}
