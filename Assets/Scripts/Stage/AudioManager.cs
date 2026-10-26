using UnityEngine;

/// <summary>
/// BGM / 효과음 / 진동 재생과 on/off 설정(PlayerPrefs) 관리.
/// Splash 씬에 두면 DontDestroyOnLoad로 모든 씬에서 유지된다.
/// 씬에 없으면(게임 씬 단독 실행 등) 소리만 안 나고 오류는 나지 않는다.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("BGM")]
    [SerializeField] private AudioClip bgmClip;

    [Header("효과음")]
    [SerializeField] private AudioClip launchClip;
    [SerializeField] private AudioClip goalClip;
    [SerializeField] private AudioClip deadClip;

    private const string BgmKey = "BgmOn";
    private const string SfxKey = "SfxOn";
    private const string VibrationKey = "VibrationOn";

    private AudioSource bgmSource;
    private AudioSource sfxSource;

    public static bool BgmOn
    {
        get => PlayerPrefs.GetInt(BgmKey, 1) == 1;
        set
        {
            Save(BgmKey, value);
            if (Instance != null) Instance.ApplyBgm();
        }
    }

    public static bool SfxOn
    {
        get => PlayerPrefs.GetInt(SfxKey, 1) == 1;
        set => Save(SfxKey, value);
    }

    public static bool VibrationOn
    {
        get => PlayerPrefs.GetInt(VibrationKey, 1) == 1;
        set => Save(VibrationKey, value);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.clip = bgmClip;
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        ApplyBgm();
    }

    public static void PlayLaunch() => PlaySfx(Instance != null ? Instance.launchClip : null);
    public static void PlayGoal() => PlaySfx(Instance != null ? Instance.goalClip : null);
    public static void PlayDead() => PlaySfx(Instance != null ? Instance.deadClip : null);

    public static void Vibrate()
    {
        if (!VibrationOn) return;
#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif
    }

    private static void PlaySfx(AudioClip clip)
    {
        if (clip == null || !SfxOn) return;
        Instance.sfxSource.PlayOneShot(clip);
    }

    private void ApplyBgm()
    {
        if (BgmOn && bgmSource.clip != null)
        {
            if (!bgmSource.isPlaying) bgmSource.Play();
        }
        else
        {
            bgmSource.Stop();
        }
    }

    private static void Save(string key, bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
        PlayerPrefs.Save();
    }
}
