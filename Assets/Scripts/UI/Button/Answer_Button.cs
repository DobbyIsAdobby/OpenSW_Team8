using UnityEngine;
using UnityEngine.UI;

public class Answer_Button : MonoBehaviour
{
    public GameObject targetObject;

    public void ActivateObject() // 버튼을 누르면 오브젝트 활성화 함수
    {
        targetObject.SetActive(true); // 오브젝트 활성화
    }

    public void DeactivateObject() // 버튼을 누르면 오브젝트 비활성화 함수
    {
        targetObject.SetActive(false); // 오브젝트 비활성화
    }

    public void Toggle()
    {
        targetObject.SetActive(!targetObject.activeSelf); // 오브젝트가 활성화 돼있으면 비활성화 또는 오브젝트가 비활성화 돼있으면 활성화
    }


    [Header("UI")]
    //public GameObject infoPanel;     // 정답+해설 창 전체
    public Text answerNameText;      // 정답 이름 Text
    public Text descriptionText;     // 정답 해설 Text

    [Header("Reference")]
    public TextureManager textureManager; // 정답/해설 정보를 가져오는 매니저

    // 버튼에 연결할 함수
    public void ShowAnswerInfo()
    {
        if (textureManager == null) return; // 매니저 없으면 종료

        // TextureManager에서 정보 가져오기
        string answerName = textureManager.GetCurrentAnswerName();
        string description = textureManager.GetCurrentAnswerDescription();

        // UI에 반영
        answerNameText.text = answerName;
        descriptionText.text = description;

        // 창 활성화
        //infoPanel.SetActive(true);
    }

    // 닫기 버튼용
    //public void HideAnswerInfo()
    //{
    //    infoPanel.SetActive(false);
    //}
}
