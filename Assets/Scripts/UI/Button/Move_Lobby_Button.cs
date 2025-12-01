using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadLobby : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnClickLoadLobby()
    {
        SceneManager.LoadScene("LobbyScene"); //로비 로드 - 씬 이름이 LobbyScene인지 확인
    }
}
