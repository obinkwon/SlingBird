/// <summary>
/// 씬이 다시 로드돼도 유지되는 게임 흐름 플래그 (정적 클래스, 씬에 붙일 필요 없음).
/// </summary>
public static class GameFlow
{
    /// <summary>true면 홈 화면을 건너뛰고 바로 게임을 시작</summary>
    public static bool SkipHome;

    /// <summary>
    /// 홈 화면 쪽에서 호출: 건너뛰어야 하면 true를 반환하고 플래그를 즉시 초기화.
    /// (앱을 새로 켰을 때는 false라서 정상적으로 홈이 뜸)
    /// </summary>
    public static bool ConsumeSkipHome()
    {
        bool value = SkipHome;
        SkipHome = false;
        return value;
    }
}
