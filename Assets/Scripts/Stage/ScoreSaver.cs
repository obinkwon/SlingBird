using UnityEngine;

/// <summary>
/// 점수 저장/불러오기 전담 정적 클래스 (PlayerPrefs 사용).
/// 씬에 붙일 필요 없이 어디서든 ScoreSaver.SaveIfBest(score) 로 호출.
/// </summary>
public static class ScoreSaver
{
    private const string BestScoreKey = "BestScore";
    private const string LastScoreKey = "LastScore";

    /// <summary>저장된 최고 점수</summary>
    public static int BestScore => PlayerPrefs.GetInt(BestScoreKey, 0);

    /// <summary>마지막 판 점수</summary>
    public static int LastScore => PlayerPrefs.GetInt(LastScoreKey, 0);

    /// <summary>
    /// 게임오버 시 호출. 마지막 점수는 항상 저장하고,
    /// 최고 점수를 넘었으면 갱신 후 true 반환.
    /// </summary>
    public static bool SaveIfBest(int score)
    {
        PlayerPrefs.SetInt(LastScoreKey, score);

        bool isNewBest = score > BestScore;
        if (isNewBest)
            PlayerPrefs.SetInt(BestScoreKey, score);

        PlayerPrefs.Save();
        return isNewBest;
    }

    /// <summary>테스트용: 저장된 기록 초기화</summary>
    public static void ResetAll()
    {
        PlayerPrefs.DeleteKey(BestScoreKey);
        PlayerPrefs.DeleteKey(LastScoreKey);
        PlayerPrefs.Save();
    }
}
