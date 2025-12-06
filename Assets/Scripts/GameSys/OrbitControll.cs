using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class OrbitControll : MonoBehaviour
{
    [Header("Target")]
    public Transform target; //큐브
    public float distance = 15.0f;

    [Header("Settings")]
    public float rotateSpeed = 0.2f;
    public float zoomSpeed = 1.0f;
    public float minDistance = 5f;
    public float maxDistance = 30f;

    private Vector2 rotationXY;
    private bool isRightClick = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //현재 카메라 각도 유지
        Vector3 angles = transform.eulerAngles;
        rotationXY.x = angles.y;
        rotationXY.y = angles.x;

        // 타겟이 없으면 0,0,0
        if(target == null)
        {
            GameObject t = new GameObject("CameraTarget");
            target = t.transform;
        }
    }

    // Update is called once per frame
    void LateUpdate() //카메라는 로직 처리 후에 움직여야 떨림 이슈가 없음
    {
        if (target == null) return;

        // 1. 쿼터니언 계산
        // 유니티의 Quaternion.Euler는 (x, y, z) 순서지만,
        // 마우스 좌우(x) 이동은 카메라의 Y축 회전이 되고,
        // 마우스 상하(y) 이동은 카메라의 X축 회전이 되니 참고
        Quaternion rotation = Quaternion.Euler(rotationXY.y, rotationXY.x, 0);

        // 2. 위치 계산: 타겟 위치에서 회전 방향으로 거리만큼 뒤로 물러남
        // (Rotation * -Vector3.forward)는 뒤쪽 방향 벡터
        Vector3 position = rotation * new Vector3(0.0f, 0.0f, -distance) + target.position;

        // 3. 적용
        transform.rotation = rotation;
        transform.position = position;
    }

    //마우스 이동
    public void OnLook(InputAction.CallbackContext context)
    {
        if (isRightClick)
        {
            Vector2 delta = context.ReadValue<Vector2>();

            rotationXY.x += delta.x * rotateSpeed;
            rotationXY.y -= delta.y * rotateSpeed;

            //상하 회전 제한. 90도 넘어가면 뒤집힘 방지
            rotationXY.y = Mathf.Clamp(rotationXY.y, -85f, 85f);
        }
    }

    //줌
    public void OnZoom(InputAction.CallbackContext context)
    {
        float scroll = context.ReadValue<float>();

        //휠을 올리면 distance가 가까워짐
        distance -= scroll * zoomSpeed * 0.5f;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
    }

    //우클릭
    public void OnRotateMode(InputAction.CallbackContext context)
    {
        if(context.started) isRightClick = true;
        if(context.canceled) isRightClick = false;
    }
}
