using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashController : MonoBehaviour
{
    [SerializeField] private CanvasGroup logo;
    [SerializeField] private float fadeTime = 0.8f;
    [SerializeField] private float holdTime = 1.2f;
    [SerializeField] private string nextScene = "Home";

    private IEnumerator Start()
    {
        logo.alpha = 0f;
        yield return Fade(0f, 1f);
        yield return new WaitForSeconds(holdTime);
        yield return Fade(1f, 0f);
        SceneManager.LoadScene(nextScene);
    }

    private IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            logo.alpha = Mathf.Lerp(from, to, t / fadeTime);
            yield return null;
        }
        logo.alpha = to;
    }
}
