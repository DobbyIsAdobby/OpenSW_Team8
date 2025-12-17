using UnityEngine;
using UnityEngine.UI;

public class Input_Answer_Button : MonoBehaviour
{
    public GameObject targetObject;
    public TextureManager textureManager; // 현재 문제의 텍스처/정답 정보를 관리하는 매니저
    // 정답/오답 UI 표시
    public GameObject successUI;
    public GameObject failUI;

    // 플레이어가 입력한 정답을 받는 InputField
    public InputField answerInputField;

    // 플레이어가 답을 제출했을 때 호출되는 함수
    public void CheckAnswer()
    {
        // 입력값에서 공백 제거
        string input = answerInputField.text.Trim();
        // 현재 정답 가져오기
        string correct = textureManager.GetCurrentAnswerName().Trim();

        // 입력과 정답 비교
        if (input == correct)
        {
            // 정답이면 성공 UI 활성화, 실패 UI 비활성화
            successUI.SetActive(true);
            failUI.SetActive(false);

            // 게임 성공 기록 갱신 (ScoreManager에서 점수 가져오기)
            RecordManager.Instance.OnGameSuccess(
                ScoreManager.Instance.score
            );

            Debug.Log("<color=green>게임 성공</color>");
        }
        else
        {
            // 오답이면 실패 UI 활성화, 성공 UI 비활성화
            successUI.SetActive(false);
            failUI.SetActive(true);

            // 게임 실패 기록 갱신
            RecordManager.Instance.OnGameFail();

            Debug.Log("<color=red>게임 실패</color>");
        }
    }

    // 오브젝트 활성화
    public void ActivateObject() => targetObject.SetActive(true);
    // 오브젝트 비활성화
    public void DeactivateObject() => targetObject.SetActive(false);
    // 오브젝트 활성/비활성 상태 토글
    public void Toggle() => targetObject.SetActive(!targetObject.activeSelf);
}
