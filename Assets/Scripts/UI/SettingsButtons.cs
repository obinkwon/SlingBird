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

    private void Start()
    {
        if (bgmButton != null) bgmButton.onClick.AddListener(ToggleBgm);
        if (sfxButton != null) sfxButton.onClick.AddListener(ToggleSfx);
        if (vibrationButton != null) vibrationButton.onClick.AddListener(ToggleVibration);

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
