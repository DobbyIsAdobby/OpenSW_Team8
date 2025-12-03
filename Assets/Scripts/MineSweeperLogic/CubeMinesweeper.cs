using UnityEngine;
using UnityEngine.InputSystem;

//3차원 (index, x, y) coordinate 사용으로 직관적 표현 진행

public class CubeMinesweeper : MonoBehaviour
{
    //setting value
    public int gridSize = 10;
    public int totalMines = 20;

    //core data - private로 외부 접근 차단
    private CellData[,,] board;

    //Cube - MeshCollider 필수
    [SerializeField] private  Transform cubeTransform;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeBoard();
        //TODO - 지뢰 심기 및 숫자 계산 기능 구현 필요
    }

    // Player Input 컴포넌트의 Events -> Fire에 연결하면됨.
    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if(Mouse.current == null) return;
            Vector2 mousePos = Mouse.current.position.ReadValue();

            ProcessRaycast(mousePos);
        }
    }

    void ProcessRaycast(Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit))
        {
            if(hit.transform != cubeTransform) return;

            int face = GetFaceFromNormal(hit.normal);
            Vector2Int gridPos = GetGridFromUV(hit.textureCoord);

            if (face != -1)
            {
                // 데이터 가져오기
                CellData clickedCell = board[face, gridPos.x, gridPos.y];
                // 데이터 변경 -> gizmo에서 빨갛게 보이게
                clickedCell.isRevealed = true;

                Debug.Log($"클릭: Face {face} / ({gridPos.x}, {gridPos.y})");
                
                // TODO: 여기에 게임 로직 추가 
            }
        }
    }

    //보드 초기화
    void InitializeBoard()
    {
        board = new CellData[6, gridSize, gridSize];

        for (int f = 0; f < 6; f++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    board[f, x, y] = new CellData(f, x, y);
                }
            }
        }
        Debug.Log("보드 생성 완료");
    }

    // normal Vector(법선 벡터)를 사용해 각 면의 index 찾기
    int GetFaceFromNormal(Vector3 normal)
    {
        // 큐브가 회전해 있을 수 있으므로 로컬 좌표로 변환해야 정확함
        Vector3 localNormal = cubeTransform.InverseTransformDirection(normal);
        
        // 정밀도를 위해 반올림
        int x = Mathf.RoundToInt(localNormal.x);
        int y = Mathf.RoundToInt(localNormal.y);
        int z = Mathf.RoundToInt(localNormal.z);

        if (y == 1) return 0;  // Top
        if (y == -1) return 1; // Bottom
        if (x == -1) return 2; // Left
        if (x == 1) return 3;  // Right
        if (z == 1) return 4;  // Front
        if (z == -1) return 5; // Back
        
        return -1; // 에러
    }

    // UV 좌표로 grid index 찾기
    Vector2Int GetGridFromUV(Vector2 uv)
    {
        int x = Mathf.FloorToInt(uv.x * gridSize);
        int y = Mathf.FloorToInt(uv.y * gridSize);
        
        // UV가 1.0일 때 배열 인덱스 초과 방지
        x = Mathf.Clamp(x, 0, gridSize - 1);
        y = Mathf.Clamp(y, 0, gridSize - 1);

        return new Vector2Int(x, y);
    }

    //core dev -> 디버그용 시각화 --gizmo사용
    //렌더링 구현 전 데이터가 맞는지 직접 확인을 해야하는 용도
    private void OnDrawGizmos()
    {
        if (board == null || cubeTransform == null) return;

        Gizmos.color = Color.red;
        float cellSize = 1.0f / gridSize; // UV 기준 한 칸 크기
        float offset = 0.5f; // 큐브 중심점 보정

        // 전체 데이터를 순회하며 isRevealed == true인 것만 그림
        for (int f = 0; f < 6; f++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    if (board[f, x, y].isRevealed)
                    {
                        // 1. 해당 셀의 로컬 중심 좌표 계산 (UV -> Local Position)
                        // 주의: 이 계산은 각 면마다 축이 달라서 복잡하지만, 단순 확인을 위해 대략적 위치만 잡음.
                        // 정확한 시각화를 하려면 별도의 변환 함수가 필요하지만, 
                        // 클릭했는지 확인용으로 면의 중심에 구체를 그림.
                        
                        // 정확한 위치 계산은 렌더링 파트에서 다룰 내용이라 지금 넣으면 코드가 너무 길어짐.
                        // 일단은 클릭된 면의 "중앙"에 큰 구체를 그려서 면 인식이 되는지 확인하는 용도
                        
                        Vector3 faceCenter = GetFaceCenterLocal(f) * offset; // 큐브 크기가 1x1x1이라고 가정 = 왜냐? 로컬에선 1*1*1 cube이기때문.
                        
                        // 로컬 -> 월드 변환
                        Vector3 worldPos = cubeTransform.TransformPoint(faceCenter);
                        Gizmos.DrawSphere(worldPos, 0.05f);
                    }
                }
            }
        }
    }

    // Gizmos용: 각 면의 중심 방향 벡터
    Vector3 GetFaceCenterLocal(int face)
    {
        switch (face) {
            case 0: return Vector3.up;    // Top
            case 1: return Vector3.down;  // Bottom
            case 2: return Vector3.left;  // Left
            case 3: return Vector3.right; // Right
            case 4: return Vector3.forward;// Front
            case 5: return Vector3.back;  // Back
            default: return Vector3.zero;
        }
    }
}
