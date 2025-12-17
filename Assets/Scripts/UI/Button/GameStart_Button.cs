using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadGame : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnClickLoadGame()
    {
        SceneManager.LoadScene("GameScene"); //게임 화면 로드 - 씬 이름이 GameScene인지 확인
    }
}
