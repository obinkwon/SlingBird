using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// BGM / 효과음 / 진동 on/off 버튼 묶음. 홈 설정 창과 게임 일시정지 창에서 같이 쓴다.
/// 버튼 라벨은 "BGM : ON" 형식으로 자동 갱신.
/// </summary>
public class SettingsButtons : MonoBehaviour
{
    [SerializeField] private Button bgmButton;
    [SerializeField] private Button sfxButton;
    [SerializeField] private Button vibrationButton;

    // 켜짐/꺼짐을 글자를 읽지 않아도 알 수 있게 버튼 색으로도 구분한다
    private static readonly Color OnColor = new Color(0.5f, 0.83f, 0.37f);
    private static readonly Color OffColor = new Color(0.72f, 0.72f, 0.72f);

    private void Start()
    {
        if (bgmButton != null) bgmButton.onClick.AddListener(ToggleBgm);
        if (sfxButton != null) sfxButton.onClick.AddListener(ToggleSfx);
        if (vibrationButton != null) vibrationButton.onClick.AddListener(ToggleVibration);
    }

    // 게임 씬에는 설정 창과 일시정지 창이 함께 있으므로, 창이 열릴 때마다 현재 값으로 맞춘다
    private void OnEnable()
    {
        RefreshLabels();
    }

    private void OnDestroy()
    {
        if (bgmButton != null) bgmButton.onClick.RemoveListener(ToggleBgm);
        if (sfxButton != null) sfxButton.onClick.RemoveListener(ToggleSfx);
        if (vibrationButton != null) vibrationButton.onClick.RemoveListener(ToggleVibration);
    }

    private void ToggleBgm()
    {
        AudioManager.BgmOn = !AudioManager.BgmOn;
        RefreshLabels();
    }

    private void ToggleSfx()
    {
        AudioManager.SfxOn = !AudioManager.SfxOn;
        RefreshLabels();
    }

    private void ToggleVibration()
    {
        AudioManager.VibrationOn = !AudioManager.VibrationOn;
        // 켰을 때 바로 한 번 울려서 확인할 수 있게
        AudioManager.Vibrate();
        RefreshLabels();
    }

    private void RefreshLabels()
    {
        SetLabel(bgmButton, "BGM", AudioManager.BgmOn);
        SetLabel(sfxButton, "SOUND", AudioManager.SfxOn);
        SetLabel(vibrationButton, "VIBRATION", AudioManager.VibrationOn);
    }

    private void SetLabel(Button button, string title, bool on)
    {
        if (button == null) return;

        if (button.image != null) button.image.color = on ? OnColor : OffColor;

        Text label = button.GetComponentInChildren<Text>();
        if (label == null) return;

        // "VIBRATION : OFF"처럼 긴 라벨이 잘리지 않게 버튼 크기에 맞춰 글자를 줄인다
        if (!label.resizeTextForBestFit)
        {
            label.resizeTextMaxSize = label.fontSize;
            label.resizeTextMinSize = 10;
            label.resizeTextForBestFit = true;
        }
        label.text = $"{title} : {(on ? "ON" : "OFF")}";
    }
}
