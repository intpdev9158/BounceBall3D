using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    [Header("Target Setting")]
    public Transform target;
    public string targetTag = "Player"; // ✅ 공 태그
    public float height = 1.0f;

    [Header("Mouse Setting")]
    public float xSpeed = 200.0f;
    public float ySpeed = 100.0f;
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    [Header("Smooth Setting")]
    public float yDamping = 2.0f;

    [Tooltip("값이 작을수록 줌이 더 부드럽게(느리게) 멈춥니다.")]
    public float zoomDamping = 5.0f;

    [Header("Zoom Setting")]
    public float zoomSpeed = 5.0f;
    public float minDistance = 2.0f;
    public float maxDistance = 20.0f;

    private float targetDistance = 5.0f;
    private float currentDistance = 5.0f;

    private float xRotation = 0.0f;
    private float yRotation = 0.0f;
    private float currentY = 0.0f;

    void Start()
    {
        // ✅ 항상 인게임처럼: 커서 숨김 + 잠금
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Vector3 angles = transform.eulerAngles;
        xRotation = angles.y;
        yRotation = angles.x;

        if (target)
            currentY = target.position.y;

        targetDistance = 5.0f;
        currentDistance = targetDistance;
    }

    void TryFindTarget()
    {
        var go = GameObject.FindGameObjectWithTag(targetTag);
        if (go) target = go.transform;
    }

    void LateUpdate()
    {
        if (!target)
        {
            // 생성 타이밍이 늦을 수도 있어서 계속 시도
            TryFindTarget();
            if (!target) return;
        }

        // ✅ 항상 회전/줌 입력 받기
        xRotation += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
        yRotation -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
        yRotation = ClampAngle(yRotation, yMinLimit, yMaxLimit);

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        targetDistance -= scroll * zoomSpeed;
        targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);

        // 따라가기
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * zoomDamping);
        currentY = Mathf.Lerp(currentY, target.position.y, Time.deltaTime * yDamping);

        Quaternion rotation = Quaternion.Euler(yRotation, xRotation, 0);
        Vector3 smoothTargetPos = new Vector3(target.position.x, currentY, target.position.z);
        Vector3 position = rotation * new Vector3(0.0f, 0.0f, -currentDistance)
                         + smoothTargetPos
                         + new Vector3(0, height, 0);

        transform.rotation = rotation;
        transform.position = position;
    }

    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f) angle += 360f;
        if (angle > 360f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }
}
