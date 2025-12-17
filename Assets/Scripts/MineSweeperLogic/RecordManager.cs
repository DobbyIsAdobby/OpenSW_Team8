using UnityEngine;

public class RecordManager : MonoBehaviour
{
    public static RecordManager Instance; // 싱글톤 인스턴스, 어디서든 RecordManager.Instance로 접근 가능

    [Header("누적 기록")]
    public int successCount; // 성공 횟수 누적
    public int failCount; // 실패 횟수 누적
    public int bestScore; // 최고 점수
    public int bestStreak; // 연속 성공 최고 기록

    [Header("현재 판 상태")]
    private int currentStreak = 0; // 현재 게임에서의 연속 성공 횟수
    private bool gameRunning = false; // 게임 진행 중 여부 체크

    void Awake()
    {
        // 부모 객체가 있으면 null로 설정하여 독립시킴
        if (transform.parent != null)
            transform.SetParent(null);

        // 싱글톤 패턴 적용
        if (Instance == null)
        {
            Instance = this; // 인스턴스 등록
            DontDestroyOnLoad(gameObject); // 씬 전환에도 파괴되지 않음
            LoadRecords(); // 저장된 기록 불러오기
        }
        else
        {
            Destroy(gameObject); // 중복 인스턴스 제거
        }
    }

    // PlayerPrefs에서 기록 불러오기
    void LoadRecords()
    {
        successCount = PlayerPrefs.GetInt("SuccessCount", 0); // 성공 횟수
        failCount = PlayerPrefs.GetInt("FailCount", 0); // 실패 횟수
        bestScore = PlayerPrefs.GetInt("BestScore", 0); // 최고 점수
        bestStreak = PlayerPrefs.GetInt("BestStreak", 0); // 연속 성공 최고 기록
    }

    // PlayerPrefs에 기록 저장
    public void SaveRecords()
    {
        PlayerPrefs.SetInt("SuccessCount", successCount);
        PlayerPrefs.SetInt("FailCount", failCount);
        PlayerPrefs.SetInt("BestScore", bestScore);
        PlayerPrefs.SetInt("BestStreak", bestStreak);
        PlayerPrefs.Save(); // 즉시 저장
    }

    // 저장한 기록 초기화
    public void ResetRecords()
    {
        // 모든 기록 초기화
        successCount = 0;
        failCount = 0;
        bestScore = 0;
        bestStreak = 0;
        currentStreak = 0;

        // PlayerPrefs 삭제
        PlayerPrefs.DeleteKey("SuccessCount");
        PlayerPrefs.DeleteKey("FailCount");
        PlayerPrefs.DeleteKey("BestScore");
        PlayerPrefs.DeleteKey("BestStreak");
        PlayerPrefs.Save();

        // 현재 인스턴스 값도 바로 반영
        SaveRecords();

        Debug.Log("<color=cyan>모든 기록이 초기화되었습니다.</color>");
    }


    // 새 게임 시작 시 호출
    public void StartNewGame()
    {
        gameRunning = true; // 게임 진행 상태 true
    }

    // 게임 성공 시 호출
    public void OnGameSuccess(int score)
    {
        if (!gameRunning) return; // 게임 중이 아니면 무시
        gameRunning = false; // 게임 종료 처리

        successCount++; // 성공 횟수 증가
        
        if (score > bestScore) // 최고 점수 갱신
            bestScore = score;
        
        currentStreak++; // 연속 성공 증가
        if (currentStreak > bestStreak) // 연속 최고 기록 갱신
            bestStreak = currentStreak;

        SaveRecords(); // 기록 저장
    }

    // 게임 실패 시 호출
    public void OnGameFail()
    {
        if (!gameRunning) return; // 게임 중이 아니면 무시
        gameRunning = false; // 게임 종료 처리

        failCount++; // 실패 횟수 증가
        currentStreak = 0; // 연속 성공 초기화

        SaveRecords(); // 기록 저장
    }
}
