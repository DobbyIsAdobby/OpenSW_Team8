using UnityEngine;

// 검증 도구 : 이 스크립트를 GameManager에 추가하고, CubeMinesweeper는 잠시 꺼두거나 같이 켜도 됨.
public class TopologyDebugger : MonoBehaviour
{
    public Transform cubeTransform;
    public int gridSize = 10;
    
    // 마지막으로 클릭한 좌표
    private int lastF = -1, lastX = -1, lastY = -1;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // CubeMinesweeper에 있는 로컬 좌표 계산 로직을 빌려옴 (정확한 위치 파악용)
                Vector3 localPoint = cubeTransform.InverseTransformPoint(hit.point);
                if (GetHitInfo(localPoint, out int f, out int x, out int y))
                {
                    lastF = f; lastX = x; lastY = y;
                    Debug.Log($"<color=yellow>[DEBUG]</color> 클릭한 곳: Face {f} ({x}, {y})");
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (lastF == -1 || cubeTransform == null) return;

        // 1. 내가 클릭한 셀 (초록색 공)
        Vector3 myPos = GetWorldPos(lastF, lastX, lastY);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(myPos, 0.5f);

        // 2. 상하좌우 이웃 확인 (파란색 선)
        // 위(Up), 아래(Down), 왼쪽(Left), 오른쪽(Right) 순서대로 선을 그립니다.
        CheckNeighbor(0, 1, Color.blue);   // 위
        CheckNeighbor(0, -1, Color.red);   // 아래
        CheckNeighbor(-1, 0, Color.yellow);// 왼쪽
        CheckNeighbor(1, 0, Color.cyan);   // 오른쪽
    }

    void CheckNeighbor(int dx, int dy, Color color)
    {
        int nFace, nX, nY;
        
        // CubeTopology에게 옆면 어딘지 물어봄
        CubeTopology.GetNeighbor(lastF, lastX, lastY, dx, dy, gridSize, out nFace, out nX, out nY);

        // 그 옆면의 실제 월드 좌표를 계산
        Vector3 neighborPos = GetWorldPos(nFace, nX, nY);

        // 선 그리기
        Gizmos.color = color;
        Gizmos.DrawLine(GetWorldPos(lastF, lastX, lastY), neighborPos);
        Gizmos.DrawSphere(neighborPos, 0.2f); // 끝점에 작은 공
    }

    // --- Visualizer의 배치 공식과 동일해야 함 ---
    // 만약 Visualizer 코드를 바꿨다면 이 부분도 똑같이 맞춰줘야 검증이 됨.
    Vector3 GetWorldPos(int f, int x, int y)
    {
        float size = 10f; 
        float cellSize = size / gridSize;
        float half = size * 0.5f;
        // x, y를 0~9 -> -4.5 ~ 4.5 로 변환
        float lx = (x - gridSize * 0.5f + 0.5f) * cellSize;
        float ly = (y - gridSize * 0.5f + 0.5f) * cellSize;
        float surf = half + 0.5f; // 표면 위

        Vector3 lp = Vector3.zero;
        Quaternion lr = Quaternion.identity;

        // Visualizer.cs의 switch문과 똑같아야 함
        switch (f) {
            case 0: lp = new Vector3(lx, surf, ly); break;   // Top
            case 1: lp = new Vector3(lx, -surf, ly); break;  // Bottom (Visualizer에서 -surf였는지 확인)
            case 2: lp = new Vector3(-surf, ly, -lx); break; // Left
            case 3: lp = new Vector3(surf, ly, lx); break;   // Right
            case 4: lp = new Vector3(lx, ly, surf); break;   // Front
            case 5: lp = new Vector3(-lx, ly, -surf); break; // Back
        }
        return cubeTransform.position + (cubeTransform.rotation * lp);
    }

    // 클릭 좌표 찾는 헬퍼 (CubeMinesweeper에서 가져옴)
    bool GetHitInfo(Vector3 p, out int f, out int x, out int y)
    {
        f = -1; x = -1; y = -1;
        float ax = Mathf.Abs(p.x), ay = Mathf.Abs(p.y), az = Mathf.Abs(p.z);
        float m = Mathf.Max(ax, Mathf.Max(ay, az));
        if (m < 0.49f) return false;
        
        float u=0, v=0;
        if (m == ay) { f = p.y>0?0:1; u=p.x; v=p.z; }
        else if (m == ax) { f = p.x>0?3:2; u=p.x>0?p.z:-p.z; v=p.y; }
        else { f = p.z>0?4:5; u=p.z>0?p.x:-p.x; v=p.y; }

        x = Mathf.Clamp(Mathf.FloorToInt((u+0.5f)*gridSize), 0, gridSize-1);
        y = Mathf.Clamp(Mathf.FloorToInt((v+0.5f)*gridSize), 0, gridSize-1);
        return true;
    }
}
