using UnityEngine;

//데이터 구조 정의를 위한 C# 클래스임. - MonoBehavior 상속 절대 금지 : 600개의 셀을 만들면 오버헤드 발생 가능성 매우 높음

[System.Serializable] //인스펙터에서 디버깅용으로 보기 위해 추가.

public class CellData
{
    public int faceIndex; // 0-5
    public int x;         // 0-9
    public int y;         // 0-9

    public bool isMine; //지뢰 여부
    public bool isRevealed; //열림 여부
    public bool isFlagged; //깃발 여부
    public int mineCount; //주변 지뢰 개수

    //생성자
    public CellData(int f, int x, int y)
    {
        this.faceIndex = f;
        this.x = x;
        this.y = y;
    }
}
