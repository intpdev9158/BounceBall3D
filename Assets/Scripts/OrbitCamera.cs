using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    [Header("Target Setting")]
    public Transform target;
    public float height = 1.0f;

    [Header("Mouse Setting")]
    public float xSpeed = 200.0f;
    public float ySpeed = 100.0f;
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    [Header("Smooth Setting")]
    public float yDamping = 2.0f; 
    
    // ⭐ 줌 댐핑(부드러움) 변수 추가
    [Tooltip("값이 작을수록 줌이 더 부드럽게(느리게) 멈춥니다.")]
    public float zoomDamping = 5.0f; 

    [Header("Zoom Setting")]
    public float zoomSpeed = 5.0f; 
    public float minDistance = 2.0f; 
    public float maxDistance = 20.0f; 

    // 목표로 하는 거리 (휠을 굴리면 이 값이 변함)
    private float targetDistance = 5.0f; 
    // 실제 카메라가 위치하는 거리 (목표 거리를 부드럽게 따라감)
    private float currentDistance = 5.0f; 

    private float xRotation = 0.0f;
    private float yRotation = 0.0f;
    private float currentY = 0.0f;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        xRotation = angles.y;
        yRotation = angles.x;

        Cursor.lockState = CursorLockMode.Locked;

        if (target) 
        {
            currentY = target.position.y;
        }

        // 시작할 때 거리 초기화
        targetDistance = 5.0f; // 기본 거리
        currentDistance = targetDistance;
    }

    // void OnEnable()
    // {
    //     PlayerRespawn.OnReset += ResetCamera;
    // }

    // void OnDisable()
    // {
    //     PlayerRespawn.OnReset -= ResetCamera;
    // }

    // public void ResetCamera()
    // {
    //     if (target == null) return;

    //     // 1. Y축 위치 즉시 동기화
    //     currentY = target.position.y;
        
    //     // 2. 줌 거리도 즉시 동기화 (죽었다 살아났는데 줌이 울렁거리면 이상하니까요)
    //     currentDistance = targetDistance;

    //     Quaternion rotation = Quaternion.Euler(yRotation, xRotation, 0);
    //     Vector3 smoothTargetPos = new Vector3(target.position.x, currentY, target.position.z);
        
    //     // currentDistance 사용
    //     Vector3 position = rotation * new Vector3(0.0f, 0.0f, -currentDistance) + smoothTargetPos + new Vector3(0, height, 0);

    //     transform.rotation = rotation;
    //     transform.position = position;
    // }

    void LateUpdate()
    {
        if (target)
        {
            // 1. 마우스 회전
            xRotation += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            yRotation -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
            yRotation = ClampAngle(yRotation, yMinLimit, yMaxLimit);

            // 2. 줌 입력 처리 (목표값 설정)
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            targetDistance -= scroll * zoomSpeed;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);

            // ⭐ 3. 줌 부드럽게 보간 (핵심!)
            // 현재 거리(current)가 목표 거리(target)를 부드럽게 따라갑니다.
            currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * zoomDamping);

            // 4. Y축 부드럽게 보간
            currentY = Mathf.Lerp(currentY, target.position.y, Time.deltaTime * yDamping);

            // 5. 최종 위치 계산
            Quaternion rotation = Quaternion.Euler(yRotation, xRotation, 0);
            Vector3 smoothTargetPos = new Vector3(target.position.x, currentY, target.position.z);

            // 여기서 -currentDistance를 사용합니다.
            Vector3 position = rotation * new Vector3(0.0f, 0.0f, -currentDistance) + smoothTargetPos + new Vector3(0, height, 0);

            transform.rotation = rotation;
            transform.position = position;
        }
    }

    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f) angle += 360f;
        if (angle > 360f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }
}