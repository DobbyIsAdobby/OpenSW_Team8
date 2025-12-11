using UnityEngine;
using System.Collections.Generic;

public class CubeVisualizer : MonoBehaviour
{
    [System.Serializable]
    public struct FaceSetting
    {
        public string name;      // 구분용 이름 - 자동으로 설정됨
        public Vector3 position; // 직접 입력할 위치
        public Vector3 rotation; // 직접 입력할 회전
    }

    [Header("Manual Settings")]
    public FaceSetting[] manualFaceSettings; 

    public TextureManager textureManager;
    public Material tileCoverMaterial;
    public Material baseImageMaterial;

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
    [Tooltip("지뢰가 너무 작으면 키우세요 (예: 100)")]
    public float mineScaleFactor = 1.0f; 
    [Tooltip("지뢰가 회전되어 있다면 조절 (예: 0, 90, 0)")]
    public Vector3 mineRotationOffset = Vector3.zero;

    [Header("Prefabs")]
    public GameObject tilePrefab;  // 타일
    public GameObject flagPrefab;  // 깃발
    public GameObject minePrefab;  // 지뢰
    public GameObject[] numberPrefabs; // 인덱스 0: 빈땅, 1~8: 숫자 모델

    // 생성된 타일들을 관리할 배열 (삭제하거나 바꾸기 위해 필요)
    private GameObject[,,] visualObjects; 

    private List<GameObject> createdBaseImages = new List<GameObject>();

    public void InitializeVisuals()
    {
        if (visualObjects != null)
        {
            foreach (var obj in visualObjects) if (obj) Destroy(obj);
        }

        if (createdBaseImages != null)
        {
            foreach (var img in createdBaseImages) if (img) Destroy(img);
            createdBaseImages.Clear();
        }

        visualObjects = new GameObject[6, gridSize, gridSize];

        // 텍스처 매니저 초기화 (랜덤 이미지 뽑기)
        if (textureManager != null) textureManager.Initialize();

        CreateBaseImages();

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

    // 6면의 그림 생성 함수 (수동 좌표 적용으로 변경됨)
    void CreateBaseImages()
    {
        // 수동 설정 데이터가 없으면 에러 로그 출력 후 중단
        if (manualFaceSettings == null || manualFaceSettings.Length < 6)
        {
            Debug.LogError("Inspector에서 'Manual Face Settings'의 Size를 6으로 설정하고 좌표를 입력해주세요.");
            return;
        }

        for (int f = 0; f < 6; f++)
        {
            GameObject faceImg = GameObject.CreatePrimitive(PrimitiveType.Quad);
            faceImg.name = $"BaseImage_Face_{f}";
            
            // 혹시 모를 collider 제거
            Destroy(faceImg.GetComponent<Collider>());
            faceImg.transform.SetParent(transform);
            
            // 리스트에 추가
            createdBaseImages.Add(faceImg);

            // 직접 좌표 지정............................................
            faceImg.transform.localPosition = manualFaceSettings[f].position;
            faceImg.transform.localRotation = Quaternion.Euler(manualFaceSettings[f].rotation);

            // 크기 설정
            faceImg.transform.localScale = new Vector3(cubeSize, cubeSize, 1);

            // 재질 적용
            Renderer r = faceImg.GetComponent<Renderer>();
            if (baseImageMaterial != null) r.material = baseImageMaterial;

            // 텍스처 입히기
            if (textureManager != null) textureManager.ApplyToFace(faceImg, f);
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
                //SpawnObject(f, x, y, minePrefab, false);
                SpawnObject(f, x, y, minePrefab, false, false, true);
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
    void SpawnObject(int f, int x, int y, GameObject prefab, bool isTile = false, bool isNumber = false, bool isMine = false)
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

            if (textureManager != null)
            {
                Renderer r = obj.GetComponentInChildren<Renderer>();
                if (r != null) r.material = tileCoverMaterial;
            }
        }
        else if (isMine)
        {
            // 지뢰
            float mScale = finalScale * mineScaleFactor;
            obj.transform.localScale = new Vector3(mScale, mScale, mScale * 0.1f);

            // 지뢰 회전 보정 적용
            obj.transform.localRotation *= Quaternion.Euler(mineRotationOffset);
        }
        else
        {
            // 숫자 및 기타
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
    void GetWorldPose(int face, float x, float y, out Vector3 position, out Quaternion rotation)
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

    // 특정 타일을 "강조(Emission)" 해서 빛나게 만드는 함수
    // face, x, y : 큐브의 (면, 좌표) 위치
    // color : 강조할 색상 (예: 파란색 → preview, 빨간색 → 지뢰 표시)
    public void FlashTile(int face, int x, int y, Color color)
    {
        // visualObjects 배열에서 해당 위치의 실제 렌더링 객체 가져오기
        var tileObj = visualObjects[face, x, y];
        if (tileObj == null) return; // 생성되지 않았으면 종료

        // 타일 prefab의 구조가
        // 타일(부모)
        // └─ Mesh 오브젝트(자식)
        // 형태일 때 부모에는 MeshRenderer가 없음 → 자식에서 MeshRenderer 가져와야 함
        var renderer = tileObj.GetComponentInChildren<MeshRenderer>(); // 자식까지 검색하여 MeshRenderer 찾기
        if (renderer == null) return; // 어차피 MeshRenderer 없으면 색을 바꿀 수 없음

        // Emission 기능을 켬 (빛나는 기능)
        renderer.material.EnableKeyword("_EMISSION");

        // emission 색상 설정 (color * 2f : 빛 강하게)
        renderer.material.SetColor("_EmissionColor", color * 2f);
    }

    // 특정 타일의 강조(Emission)를 제거하여 원래 상태로 되돌리는 함수
    public void UnflashTile(int face, int x, int y)
    {
        // (face, x, y)에 해당하는 타일 GameObject 가져오기
        var tileObj = visualObjects[face, x, y];
        if (tileObj == null) return;

        // FlashTile과 마찬가지로 자식까지 포함하여 MeshRenderer 찾기
        var renderer = tileObj.GetComponentInChildren<MeshRenderer>(); // ★ 변경
        if (renderer == null) return;

        // Emission 비활성화 → 더 이상 빛나지 않음
        renderer.material.DisableKeyword("_EMISSION");

        // 검정(0)으로 emission 색상을 초기화하여 강조 제거
        renderer.material.SetColor("_EmissionColor", Color.black);
    }
}