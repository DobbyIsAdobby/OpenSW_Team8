using UnityEngine;
using UnityEngine.InputSystem;

//3차원 (index, x, y) coordinate 사용으로 직관적 표현 진행
//251209기준 데이터와 로직은 구현완료.. 이걸 이제 MVC 패턴으로 Visualizer를 구현하면될듯

public class CubeMinesweeper : MonoBehaviour
{
    public CubeVisualizer visualizer;

    //setting value
    public int gridSize = 10;
    public int totalMines = 100;

    //core data - private로 외부 접근 차단
    private CellData[,,] board;

    //첫번째로 클릭했는지 확인할 이벤트 변수가 필요함.
    private bool isFirstClick = true;

    //Cube - MeshCollider 필수
    [SerializeField] private  Transform cubeTransform;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeBoard();
        if (visualizer) visualizer.InitializeVisuals();
        //TODO - 지뢰 심기 및 숫자 계산 기능 구현 필요 - 구현 완료
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

    public void OnFlag(InputAction.CallbackContext context)
    {
        // 스페이스바를 눌렀을 때 실행
        if (context.performed) 
        {
            // 마우스가 가리키고 있는 셀을 찾아서 깃발 처리
            if (TryGetHitCell(out int f, out int x, out int y))
            {
                HandleFlag(f, x, y);
            }
        }
    }

    // 마우스가 현재 가리키고 있는 3D 타일의 좌표를 찾아주는 함수
    bool TryGetHitCell(out int face, out int x, out int y)
    {
        face = -1; x = -1; y = -1;

        if (Mouse.current == null) return false;
        
        // 스페이스바를 눌렀더라도, 위치 기준은 마우스 위치입니다.
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 localPoint = cubeTransform.InverseTransformPoint(hit.point);
            Vector2Int gridPos;
            
            if (GetHitInfoFromLocalPoint(localPoint, out face, out gridPos))
            {
                x = gridPos.x;
                y = gridPos.y;
                return true;
            }
        }
        return false;
    }

    void HandleFlag(int face, int x, int y)
    {
        CellData cell = board[face, x, y];

        // 이미 열린 곳에는 깃발을 못 꽂음
        if (cell.isRevealed) return;

        // 깃발 상태 토글 (ON - OFF)
        cell.isFlagged = !cell.isFlagged;

        // 비주얼 업데이트
        if (visualizer) visualizer.UpdateVisual(face, x, y, cell);

        if (cell.isFlagged) Debug.Log($"<color=yellow>깃발 설치</color> ({face}, {x},{y})");
        else Debug.Log($"<color=grey>깃발 해제</color> ({face}, {x},{y})");
    }

    void HandleReveal(int face, int x, int y)
    {
        CellData cell = board[face, x, y];

        // 이미 열렸거나, 깃발이 꽂혀있으면 클릭 무시
        if (cell.isRevealed || cell.isFlagged) return;

        if (isFirstClick)
        {
            GenerateMines(face, x, y);
            isFirstClick = false;
        }

        if (cell.isMine)
        {
            cell.isRevealed = true;
            if (visualizer) visualizer.UpdateVisual(face, x, y, cell);
            Debug.Log($"<color=red>펑! 게임 오버</color> ({face}, {x},{y})");
        }
        else if (cell.mineCount == 0)
        {
            FloodFill(face, x, y);
        }
        else
        {
            cell.isRevealed = true;
            if (visualizer) visualizer.UpdateVisual(face, x, y, cell);
        }
    }

    //단순 함수만 호출하게 재수정 - 첫번째 클릭 확인으로 로직 수정 후
    void ProcessRaycast(Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit))
        {
            /*if(hit.transform != cubeTransform) return;

            int face = GetFaceFromNormal(hit.normal);
            Vector2Int gridPos = GetGridFromUV(hit.textureCoord);

            if (face != -1)
            {
                // 데이터 가져오기
                //CellData clickedCell = board[face, gridPos.x, gridPos.y];
                // 데이터 변경 -> gizmo에서 빨갛게 보이게
                //clickedCell.isRevealed = true;

                //Debug.Log($"클릭: Face {face} / ({gridPos.x}, {gridPos.y})");
                
                // TODO: 여기에 게임 로직 추가 
                HandleCellClick(face, gridPos.x, gridPos.y);
            }*/

            // UV 좌표 대신, 충돌한 지점의 로컬 좌표 사용.
            // 월드 좌표 -> 큐브 기준 로컬 좌표로 변환
            Vector3 localPoint = cubeTransform.InverseTransformPoint(hit.point);

            int face;
            Vector2Int gridPos;

            // 로컬 좌표를 분석해 정확한 index를 찾아내는 함수 호출
            if (GetHitInfoFromLocalPoint(localPoint, out face, out gridPos))
            {
                HandleCellClick(face, gridPos.x, gridPos.y);
            }
        }
    }

    // core dev -> Visualizer의 배치 공식과 정반대로 계산하여 인덱스를 추출
    bool GetHitInfoFromLocalPoint(Vector3 localPoint, out int face, out Vector2Int gridPos)
    {
        face = -1;
        gridPos = Vector2Int.zero;

        // 큐브의 로컬 크기는 -0.5 ~ 0.5 범위라고 가정하고 계산 (Scale은 TransformPoint에서 처리되므로)
        float absX = Mathf.Abs(localPoint.x);
        float absY = Mathf.Abs(localPoint.y);
        float absZ = Mathf.Abs(localPoint.z);

        // 가장 값이 큰 축이 곧 충돌한 면의 방향
        float maxAxis = Mathf.Max(absX, Mathf.Max(absY, absZ));

        // 큐브 내부 클릭 방지 (표면 근처가 아니면 무시)
        if (maxAxis < 0.49f) return false; 

        // u, v는 해당 면에서의 가로/세로 로컬 좌표 (-0.5 ~ 0.5)
        float u = 0; 
        float v = 0; 

        // Visualizer.cs의 switch(face) 로직을 그대로 역산.
        if (maxAxis == absY) // Top or Bottom
        {
            if (localPoint.y > 0) // Face 0: Top
            {
                face = 0;
                // Visualizer: new Vector3(localX, surfaceDist, localY);
                u = localPoint.x; 
                v = localPoint.z; 
            }
            else // Face 1: Bottom
            {
                face = 1;
                // Visualizer: new Vector3(localX, -surfaceDist, localY);
                // Visualizer 코드에서 Bottom은 X, Z를 그대로 썼으니 그대로 사용.
                u = localPoint.x; 
                v = localPoint.z; 
            }
        }
        else if (maxAxis == absX) // Left or Right
        {
            if (localPoint.x > 0) // Face 3: Right
            {
                face = 3;
                // Visualizer: new Vector3(surfaceDist, localY, localX);
                // Z축(localPoint.z)이 localX(그리드 가로) 역할을 함
                // Y축(localPoint.y)이 localY(그리드 세로) 역할을 함
                u = localPoint.z; 
                v = localPoint.y; 
            }
            else // Face 2: Left
            {
                face = 2;
                // Visualizer: new Vector3(-surfaceDist, localY, -localX);
                // Z축(localPoint.z)이 -localX 역할 -> 즉 u는 -z
                u = -localPoint.z;
                v = localPoint.y;
            }
        }
        else // Front or Back (Z축)
        {
            if (localPoint.z > 0) // Face 4: Front
            {
                face = 4;
                // Visualizer: new Vector3(localX, localY, surfaceDist);
                u = localPoint.x;
                v = localPoint.y;
            }
            else // Face 5: Back
            {
                face = 5;
                // Visualizer: new Vector3(-localX, localY, -surfaceDist);
                // X축(localPoint.x)이 -localX 역할 -> 즉 u는 -x
                u = -localPoint.x;
                v = localPoint.y;
            }
        }

        // 추출한 u, v (-0.5 ~ 0.5)를 그리드 인덱스 (0 ~ 9)로 변환
        // (좌표 + 0.5) * 그리드크기
        float gridU = (u + 0.5f) * gridSize;
        float gridV = (v + 0.5f) * gridSize;

        int x = Mathf.FloorToInt(gridU);
        int y = Mathf.FloorToInt(gridV);

        // 경계선 오차 보정
        x = Mathf.Clamp(x, 0, gridSize - 1);
        y = Mathf.Clamp(y, 0, gridSize - 1);

        gridPos = new Vector2Int(x, y);
        return true;
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
        visualizer.InitializeVisuals();
    }

    // normal Vector(법선 벡터)를 사용해 각 면의 index 찾기 - 더이상 사용하지 않음. 로컬 좌표계를 이용해 계산.
    /*int GetFaceFromNormal(Vector3 normal)
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
    }*/

    // UV 좌표로 grid index 찾기
    /*Vector2Int GetGridFromUV(Vector2 uv)
    {
        int x = Mathf.FloorToInt(uv.x * gridSize);
        int y = Mathf.FloorToInt(uv.y * gridSize);
        
        // UV가 1.0일 때 배열 인덱스 초과 방지
        x = Mathf.Clamp(x, 0, gridSize - 1);
        y = Mathf.Clamp(y, 0, gridSize - 1);

        return new Vector2Int(x, y);
    }*/

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

    // 좌표를 받아서 게임 규칙대로 처리하는 함수
    void HandleCellClick(int face, int x, int y)
    {
        CellData cell = board[face, x, y];

        // 1. 예외 처리: 이미 열렸거나 깃발이 꽂혀있으면 클릭 무시
        if (cell.isRevealed || cell.isFlagged) return;

        // 2. 첫 클릭 처리: 게임 시작 후 처음 눌렀을 경우
        if (isFirstClick)
        {
            // 현재 클릭한 곳(face, x, y)을 제외하고 지뢰를 심음
            GenerateMines(face, x, y);
            isFirstClick = false; // 이제 첫 클릭 아님
            Debug.Log("첫 클릭 : 지뢰 배치 완료.");
        }

        // 3. 상태 변경 - 타일 공개
        /*cell.isRevealed = true;

        // visual update
        if(visualizer) visualizer.UpdateVisual(face, x, y, cell);

        // 4. 결과 판정
        if (cell.isMine)
        {
            Debug.Log($"<color=red>게임 오버</color> (Face:{face}, {x},{y})");
            // TODO: 게임 오버 연출 함수 호출
        }
        else
        {
            Debug.Log($"<color=green>안전 지대</color> 주변 지뢰: {cell.mineCount}개");
            
            // 만약 주변 지뢰가 0개라면? -> 주변 타일도 자동으로 열어야 함 (재귀 함수/Flood Fill)
            if (cell.mineCount == 0)
            {
                // FloodFill(face, x, y); // 이건 다음 단계에서 구현할 예정
            }
        }*/

        // 경우 1: 지뢰를 밟음 -> 게임 오버
        if (cell.isMine)
        {
            cell.isRevealed = true;
            if (visualizer) visualizer.UpdateVisual(face, x, y, cell);
            Debug.Log($"<color=red>게임 오버/color> ({face}, {x},{y})");
            // TODO: 게임오버 UI 호출
        }
        // 경우 2: 빈 땅(0)임 -> Flood Fill 발동!
        else if (cell.mineCount == 0)
        {
            // FloodFill 함수가 "열기 + 비주얼 업데이트 + 주변 열기"를 다 해줍니다.
            FloodFill(face, x, y); 
            Debug.Log($"<color=cyan>안전 지대 개방</color> ({face}, {x},{y})");
        }
        // 경우 3: 그냥 숫자임 -> 해당 칸만 염
        else
        {
            cell.isRevealed = true;
            if (visualizer) visualizer.UpdateVisual(face, x, y, cell);
            Debug.Log($"<color=green>안전 지대 (숫자)</color> ({face}, {x},{y})");
        }
    }

    //지뢰 심기
    void GenerateMines(int safeFace, int safeX, int safeY)
    {
        int minesPlaced = 0;
        while (minesPlaced < totalMines)
        {
            int f = Random.Range(0, 6);
            int x = Random.Range(0, gridSize);
            int y = Random.Range(0, gridSize);

            // 이미 지뢰거나, 첫 클릭한 곳 주변이면 스킵
            if (board[f, x, y].isMine) continue;
            if (f == safeFace && x == safeX && y == safeY) continue; 
            
            board[f, x, y].isMine = true;
            minesPlaced++;
        }
        
        // 지뢰 배치가 끝났으니 숫자 계산
        CalculateNumbers();
    }

    //주변 지뢰 개수 계산
    void CalculateNumbers()
    {
        // 모든 셀을 순회
        for (int f = 0; f < 6; f++) {
            for (int x = 0; x < gridSize; x++) {
                for (int y = 0; y < gridSize; y++) {
                    
                    if (board[f, x, y].isMine) continue;

                    int count = 0;
                    
                    // 8방향 검사 (x: -1~1, y: -1~1)
                    for (int dx = -1; dx <= 1; dx++) {
                        for (int dy = -1; dy <= 1; dy++) {
                            if (dx == 0 && dy == 0) continue;

                            int nFace, nX, nY;
                            // 위상 수학을 통해 이웃 좌표를 얻음
                            CubeTopology.GetNeighbor(f, x, y, dx, dy, gridSize, out nFace, out nX, out nY);
                            
                            if (nFace >= 0 && nFace < 6 && 
                                nX >= 0 && nX < gridSize && 
                                nY >= 0 && nY < gridSize)
                            {
                                if (board[nFace, nX, nY].isMine) count++;
                            }
                        }
                    }
                    board[f, x, y].mineCount = count; // CellData에 mineCount 변수 추가 필요
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

    // 빈 땅일 때 주변을 연쇄적으로 여는 재귀 함수
    void FloodFill(int face, int x, int y)
    {
        // 1. 범위 체크
        if (face < 0 || face >= 6 || x < 0 || x >= gridSize || y < 0 || y >= gridSize) return;

        CellData cell = board[face, x, y];

        // 2. 멈춰야하는 경우
        // 이미 열렸거나, 깃발이 꽂혀있거나, 지뢰면 중단
        if (cell.isRevealed || cell.isFlagged || cell.isMine) return;

        // 3. 현재 셀 열기
        cell.isRevealed = true;
        if (visualizer) visualizer.UpdateVisual(face, x, y, cell);

        // 4. 만약 현재 셀이 숫자를 가지고 있다면(1~8), 여기서 멈춤
        if (cell.mineCount > 0) return;

        // 5. 현재 셀이 0이라면? -> 주변 8방향으로 전파
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue; // 자신은 제외

                int nFace, nX, nY;
                
                // 옆면 확인
                CubeTopology.GetNeighbor(face, x, y, dx, dy, gridSize, out nFace, out nX, out nY);

                // 유효한 면이라면 재귀 호출
                if (nFace != -1)
                {
                    FloodFill(nFace, nX, nY);
                }
            }
        }
    }
}
