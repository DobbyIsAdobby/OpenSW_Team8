using UnityEngine;

public class Menu_Button : MonoBehaviour
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
}
