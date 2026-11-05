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
        // 안드로이드 기본값은 30fps. 첫 씬에서 한 번만 지정하면 씬이 바뀌어도 유지된다
        Application.targetFrameRate = 60;

        // 로고가 없으면 연출 없이 바로 다음 씬으로 (앱 기본 스플래시만 보여주는 경우)
        if (logo != null)
        {
            logo.alpha = 0f;
            yield return Fade(0f, 1f);
            yield return new WaitForSeconds(holdTime);
            yield return Fade(1f, 0f);
        }
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
