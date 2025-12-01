using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; //씬 관리 관련 라이브러리

public class ReStartButtom : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnClickReStart() //클릭시 게임씬 로드
    {
        SceneManager.LoadScene("GameScene"); //GameScene을 로드 - 씬 이름이 GameScene인지 확인
    }
}
