using UnityEngine;
using UnityEngine.UI;

public class LobbyRecordUI : MonoBehaviour
{
    // UI 텍스트 컴포넌트
    public Text successText; // 성공 횟수 표시
    public Text failText; // 실패 횟수 표시
    public Text bestScoreText; // 최고 점수 표시
    public Text bestStreakText; // 연속 성공 최고 기록 표시

    // 오브젝트가 활성화될 때 자동으로 기록 UI 갱신
    void OnEnable()
    {
        // RecordManager 싱글톤이 존재하면 Refresh() 호출
        if (RecordManager.Instance != null)
            Refresh();
    }

    // 기록 UI를 갱신하는 함수
    public void Refresh()
    {
        // RecordManager가 없으면 종료
        if (RecordManager.Instance == null) return;

        var r = RecordManager.Instance; // 현재 기록 가져오기

        // 각 텍스트 UI에 기록 값 적용
        successText.text = $"성공 횟수 : {r.successCount}";
        failText.text = $"실패 횟수 : {r.failCount}";
        bestScoreText.text = $"최고 점수 : {r.bestScore}";
        bestStreakText.text = $"연속 성공 최고 기록 : {r.bestStreak}";
    }
}
