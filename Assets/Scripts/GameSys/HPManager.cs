using UnityEngine;
using UnityEngine.UI;

public class HPManager : MonoBehaviour
{
    public static HPManager Instance;

    [Header("Settings")]
    public float maxHP = 100f; // 최대 신뢰도
    public float currentHP;    // 현재 신뢰도

    [Header("UI")]
    public Slider hpSlider;    // 유니티 UI 슬라이더 연결
    public Image fillImage;    // 슬라이더 색상 변경용 (선택사항)
    public Color normalColor = Color.green;
    public Color warningColor = Color.red;
    public GameObject failUI;

    [Header("Game State")]
    public bool isGameOver = false; // 게임 오버 상태 체크

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 게임 시작 시 초기화
        InitializeHP();
    }

    public void InitializeHP()
    {
        currentHP = maxHP;
        isGameOver = false;
        UpdateUI();
    }

    // 데미지 입는 함수 (지뢰 밟았을 때 호출)
    public void TakeDamage(float damage)
    {
        if (isGameOver) return; // 이미 끝났으면 무시

        currentHP -= damage;
        if (currentHP <= 0)
        {
            currentHP = 0;
            TriggerGameOver();
        }
        UpdateUI();
    }

    void UpdateUI()
    {
        if (hpSlider != null)
        {
            hpSlider.value = currentHP / maxHP; // 0.0 ~ 1.0 비율

            // 체력이 낮으면 색상 변경 (30% 미만)
            if (fillImage != null)
            {
                fillImage.color = (currentHP / maxHP < 0.3f) ? warningColor : normalColor;
            }
        }
    }

    void TriggerGameOver()
    {
        isGameOver = true;
        Debug.Log("<color=red>신뢰도 0</color>");
        
        // 여기에 나중에 Game Over UI 팝업을 띄우는 코드를 추가하면 됩니다.
        // 예: UIManager.Instance.ShowGameOverPopup();
        failUI.SetActive(true);
        
        if (RecordManager.Instance != null)
        {
            RecordManager.Instance.OnGameFail();
        }
    }
}
