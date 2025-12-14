using UnityEngine;
using UnityEngine.UI;

// 게임 전체에서 점수를 관리하는 ScoreManager
public class ScoreManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    // 어디서든 ScoreManager.Instance 로 접근 가능
    public static ScoreManager Instance;

    [Header("Score")]
    public int score = 0; // 현재 점수 - 시작은 0점

    [Header("UI")]
    public Text scoreText; // 점수 UI

    // 오브젝트가 생성될 때 가장 먼저 실행되는 함수
    void Awake()
    {
        // 싱글톤 패턴
        // 인스턴스가 없다면 자신을 등록
        if (Instance == null)
            Instance = this;
        else // 이미 존재한다면 중복 생성 방지
            Destroy(gameObject);
    }

    // 게임 시작 시 한 번 실행
    void Start()
    {
        // 게임 시작하자마자 점수 UI를 갱신 (0점 표시)
        UpdateUI(); // 게임 시작시 0점 표시
    }

    // 점수를 0으로 초기화하는 함수
    // 게임 다시 시작, 새 게임 시작 등에 사용
    public void ResetScore()
    {
        score = 0;
        UpdateUI();
    }

    // 점수를 추가하거나 감소시키는 함수
    // value : 더할 점수 (음수면 감소)
    public void AddScore(int value)
    {
        score += value;
        if (score < 0) score = 0; // 점수가 0보다 더 내려가지 않게 제한
        UpdateUI();
    }

    // 점수 UI를 실제로 화면에 반영하는 함수
    void UpdateUI()
    {
        // scoreText가 인스펙터에서 정상적으로 연결되어 있다면
        if (scoreText != null)
            scoreText.text = $"점수 : {score}";
        else // 연결이 안 되었을 경우 경고 출력
            Debug.LogWarning("ScoreText not Connect");
    }
}
