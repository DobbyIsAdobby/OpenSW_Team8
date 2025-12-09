using UnityEngine;

public class CubeVisualizer : MonoBehaviour
{
    [Header("Settings")]
    public Transform targetCube;
    public int gridSize = 10;
    public float cubeSize = 10f;
    public float offsetHeight = 0.5f; // 큐브 표면에서 살짝 띄울 높이 (겹침 방지)

    [Header("Model Adjustment")]
    [Tooltip("타일/숫자 모델의 기본 크기를 보정합니다 (예: 0.1 or 0.01)")]
    public float scaleMultiplier = 1.0f; 
    [Tooltip("타일의 두께를 얇게 만듭니다")]
    public float thicknessMultiplier = 0.5f; 
    [Tooltip("숫자가 누워있으면 이 값을 조절하세요 (예: 90 or -90)")]
    public Vector3 numberRotationOffset = new Vector3(-90, 0, 0);

    [Header("Prefabs")]
    public GameObject tilePrefab;  // 타일
    public GameObject flagPrefab;  // 깃발
    public GameObject minePrefab;  // 지뢰
    public GameObject[] numberPrefabs; // 인덱스 0: 빈땅, 1~8: 숫자 모델

    // 생성된 타일들을 관리할 배열 (삭제하거나 바꾸기 위해 필요)
    private GameObject[,,] visualObjects; 

    public void InitializeVisuals()
    {
        if (visualObjects != null)
        {
            foreach (var obj in visualObjects) if (obj) Destroy(obj);
        }

        visualObjects = new GameObject[6, gridSize, gridSize];

        // 600개의 타일을 큐브 표면에 생성
        for (int f = 0; f < 6; f++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    SpawnObject(f, x, y, tilePrefab, true);
                }
            }
        }
    }

    // 특정 좌표의 visual을 업데이트하는 함수
    public void UpdateVisual(int f, int x, int y, CellData data)
    {
        // 1. 기존 타일 제거
        if (visualObjects[f, x, y] != null)
        {
            Destroy(visualObjects[f, x, y]);
        }

        // 2. 상태에 따라 새 모델 생성
        if (data.isFlagged)
        {
            SpawnObject(f, x, y, flagPrefab, false);
        }
        else if (data.isRevealed)
        {
            if (data.isMine)
            {
                SpawnObject(f, x, y, minePrefab, false);
            }
            else
            {
                // 숫자에 맞는 모델 생성 (0~8)
                // numberPrefabs 배열 크기 체크 필수
                int count = data.mineCount;
                if (count > 0 && (count - 1) < numberPrefabs.Length)
                {
                     // 배열은 0부터 시작하므로 (개수 - 1)을 해야 정확하게 가져옴.
                     SpawnObject(f, x, y, numberPrefabs[count - 1], false, true);
                }
            }
        }
        else
        {
            // 다시 덮기
            SpawnObject(f, x, y, tilePrefab, true);
        }
    }

    // 오브젝트 생성
    void SpawnObject(int f, int x, int y, GameObject prefab, bool isTile = false, bool isNumber = false)
    {
        if (prefab == null) return;

        // 위치와 회전 계산
        GetWorldPose(f, x, y, out Vector3 pos, out Quaternion rot);

        // 생성 및 부모 설정
        GameObject obj = Instantiate(prefab, pos, rot);
        obj.transform.SetParent(transform); 
        
        // 1. 그리드 한 칸 크기에 맞게 스케일 강제 조정
        float cellSize = cubeSize / gridSize;
        float finalScale = cellSize * scaleMultiplier; 
        
        // 타일이라면 Z축(두께)을 납작하게 만듦
        if (isTile)
        {
            obj.transform.localScale = new Vector3(finalScale * 0.5f, finalScale* thicknessMultiplier, finalScale * 0.5f);
        }
        else
        {
            obj.transform.localScale = new Vector3(finalScale, finalScale, finalScale);
        }

        // 2. 숫자라면 회전 보정 적용
        if (isNumber)
        {
            // 현재 회전값 * 보정 회전값
            obj.transform.localRotation *= Quaternion.Euler(numberRotationOffset);
        }

        visualObjects[f, x, y] = obj;
    }

    // (Face, x, y) -> (World Position, World Rotation)
    void GetWorldPose(int face, int x, int y, out Vector3 position, out Quaternion rotation)
    {
        // 1. gird 한 칸의 크기 (큐브 크기 / 그리드 개수)
        float cellSize = cubeSize / gridSize;
        
        // 2. 해당 면에서의 2D 로컬 좌표 (중심 기준)
        // (x, y)는 0~9 정수이므로, 이를 -0.5 ~ +0.5 범위의 좌표로 변환
        float localX = (x - (gridSize - 1) * 0.5f) * cellSize;
        float localY = (y - (gridSize - 1) * 0.5f) * cellSize;
        
        // 3. 면의 중심에서 표면까지의 거리 (큐브 크기의 절반)
        float surfaceDist = (cubeSize * 0.5f) + offsetHeight;

        // 4. 면에 따른 3D 회전과 위치 정의
        Vector3 localPos = Vector3.zero;
        Quaternion localRot = Quaternion.identity;

        switch (face)
        {
            // 주의: Unity Cube의 UV 매핑 방향과 일치시켜야 함.
            // 필요시 각도나 축(x, y)을 미세 조정해야 할 수 있음.
            case 0: // Top (y+)
                localPos = new Vector3(localX, surfaceDist, localY);
                localRot = Quaternion.Euler(0, 0, 0); // 눕혀진 상태가 기본인 경우 90,0,0 등 조정 필요
                break;
            case 1: // Bottom (y-)
                localPos = new Vector3(localX, -surfaceDist, localY); // X축 반전 여부 확인 필요
                localRot = Quaternion.Euler(180, 0, 0);
                break;
            case 2: // Left (x-)
                localPos = new Vector3(-surfaceDist, localY, -localX); // Z축 방향 주의
                localRot = Quaternion.Euler(0, 0, 90);
                break;
            case 3: // Right (x+)
                localPos = new Vector3(surfaceDist, localY, localX); 
                localRot = Quaternion.Euler(0, 0, -90);
                break;
            case 4: // Front (z+)
                localPos = new Vector3(localX, localY, surfaceDist);
                localRot = Quaternion.Euler(90, 0, 0);
                break;
            case 5: // Back (z-)
                localPos = new Vector3(-localX, localY, -surfaceDist); // X축 반전 (뒤에서 보니까)
                localRot = Quaternion.Euler(-90, 0, 0);
                break;
        }

        position = targetCube.position + (targetCube.rotation * localPos);
        rotation = targetCube.rotation * localRot;
    }
}
