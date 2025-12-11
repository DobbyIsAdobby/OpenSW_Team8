using UnityEngine;
using UnityEngine.InputSystem;

public class Pause_Button : MonoBehaviour
{
    public InputActionReference fireAction; // 왼쪽 클릭 입력 액션
    public InputActionReference flagAction; // 스페이스바 깃발 입력 액션

    private bool isPaused = false; // 입력 정지 bool 변수

    public void StopButton()
    {
        isPaused = true;
        Time.timeScale = 0f; // 게임 로직 멈춤

        fireAction.action.Disable(); // 마우스 클릭 비활성화
        flagAction.action.Disable(); // 스페이스바 비활성화
    }

    public void ResumeButton()
    {
        isPaused = false;
        Time.timeScale = 1f; // 게임 로직 다시 시작

        fireAction.action.Enable(); // 마우스 클릭 활성화
        flagAction.action.Enable(); // 스페이스바 활성화
    }
}
