using UnityEngine;

[System.Serializable]
public class AnswerTexture
{
    public Texture2D texture;   // 실제 이미지
    public string answerName;   // 정답 이름 (한글)
    [TextArea(3, 5)]
    public string description;  // 정답 해설 (한글)
}

public class TextureManager : MonoBehaviour
{
    [Header("Texture Settings")]
    public AnswerTexture[] randomTextures; // 랜덤으로 선택할 텍스처 배열
    //public Texture2D[] randomTextures;

    [Header("Layout Settings")]
    public int cols = 3; // 가로 분할 수
    public int rows = 2; // 세로 분할 수

    // 현재 선택된 텍스처와 계산된 레이아웃
    //private Texture2D currentTexture;
    private AnswerTexture currentAnswerTexture; // 현재 선택된 정답 텍스처
    private Rect[] faceRects; // 큐브 6면의 UV 영역
    private MaterialPropertyBlock propBlock; // 렌더러에 텍스처를 적용할 때 사용

    // 초기화 함수 (게임 시작 시 호출)
    public void Initialize()
    {
        propBlock = new MaterialPropertyBlock();

        // 1. 랜덤 텍스처 선택
        if (randomTextures != null && randomTextures.Length > 0)
        {
            //currentTexture = randomTextures[Random.Range(0, randomTextures.Length)];
            currentAnswerTexture = randomTextures[Random.Range(0, randomTextures.Length)];
        }

        // 2. 6면 레이아웃 계산 (3x2 구조 기준)
        CalculateLayouts();
    }

    // 3x2 구조에 맞춰 각 면의 UV 좌표(Rect)를 미리 계산
    void CalculateLayouts()
    {
        faceRects = new Rect[6];
        float w = 1.0f / cols;
        float h = 1.0f / rows;

        // [3x2 매핑 규칙] 
        // 0:Top, 1:Bottom, 2:Left, 3:Right, 4:Front, 5:Back
        faceRects[0] = new Rect(1 * w, 1 * h, w, h); // Top (중앙 위)
        faceRects[1] = new Rect(1 * w, 0 * h, w, h); // Bottom (중앙 아래)
        faceRects[2] = new Rect(0 * w, 0 * h, w, h); // Left (왼쪽 아래)
        faceRects[3] = new Rect(2 * w, 1 * h, w, h); // Right (오른쪽 위)
        faceRects[4] = new Rect(0 * w, 1 * h, w, h); // Front (왼쪽 위 - 메인)
        faceRects[5] = new Rect(2 * w, 0 * h, w, h); // Back (오른쪽 아래)
    }

    public void ApplyToFace(GameObject faceObj, int face)
    {
        Renderer r = faceObj.GetComponent<Renderer>();
        //if (r == null || currentTexture == null) return;
        if (r == null || currentAnswerTexture == null) return;

        Rect rect = faceRects[face];

        r.GetPropertyBlock(propBlock);
        //propBlock.SetTexture("_MainTex", currentTexture);
        propBlock.SetTexture("_MainTex", currentAnswerTexture.texture);
        // 면 전체를 덮으므로 rect의 크기와 위치를 그대로 타일/오프셋으로 사용
        propBlock.SetVector("_MainTex_ST", new Vector4(rect.width, rect.height, rect.x, rect.y));
        r.SetPropertyBlock(propBlock);
    }

    // 이미지 정답 이름을 꺼내는 함수
    public string GetCurrentAnswerName()
    {
        if (currentAnswerTexture == null)
            return "";

        return currentAnswerTexture.answerName;
    }
    
    
    // 이미지 정답의 해설을 꺼내는 함수
    public string GetCurrentAnswerDescription()
    {
        if (currentAnswerTexture == null)
            return "";

        return currentAnswerTexture.description;
    }

}
