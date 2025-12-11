using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class BounceBallMovement : MonoBehaviour
{
    [Header("Master Control")]
    public float bouncesPerSecond = 1.5f;  // 초당 튕기는 횟수
    public float jumpHeight       = 3.0f;  // 기본 점프 높이

    [Header("Movement Control")]
    public float maxDistance   = 3.0f;     // 기본 점프 시 최대 이동 거리(칸 기준 3칸)
    public float acceleration  = 5.0f;
    public float stopFriction  = 5.0f;     // 착지 시 감속 강도

    public Transform cameraTransform;

    private Rigidbody rb;
    private bool isGrounded;

    // 내부 계산값
    private float calculatedGravity;
    private float jumpVelocity;
    private float targetMoveSpeed;

    // 점프패드용 수평 속도 배수 (기본 1)
    private float horizontalSpeedMultiplier = 1f;

    [Header("Dash Control")]
    public float dashSpeed = 5f;   // ← dashSpped 오타 수정
    private bool isDashByBlock = false;
    private Vector3 dashDir;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.freezeRotation = true;
        rb.useGravity     = false;
        rb.linearDamping  = 0;

        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        SyncPhysics();
    }

    void OnValidate()
    {
        if (rb != null) SyncPhysics();
    }

    /// <summary>
    /// bouncesPerSecond, jumpHeight, maxDistance 를 기반으로
    /// 중력, 점프속도, 수평이동속도를 계산
    /// </summary>
    void SyncPhysics()
    {
        float timeInAir  = 1.0f / bouncesPerSecond;  // 한 번 점프 시 총 체공 시간
        float timeToApex = timeInAir / 2.0f;         // 최고점까지 걸리는 시간

        // s = 1/2 g t^2  → g = 2s / t^2
        calculatedGravity = (2f * jumpHeight) / (timeToApex * timeToApex);

        // v = g t
        jumpVelocity = calculatedGravity * timeToApex;

        // 한 번 점프 동안 maxDistance 만큼 이동하도록 수평 속도 설정
        targetMoveSpeed = maxDistance / timeInAir;
    }

    void FixedUpdate()
    {   
        // ★ 대쉬 블록에 의해 이동 중이면
        if (isDashByBlock)
        {
            // 중력 적용하지 않고, 일정 속도로 직선 이동
            rb.linearVelocity = new Vector3(
                dashDir.x * dashSpeed,
                0f,
                dashDir.z * dashSpeed
            );

            // 아무 키나 누르면 대쉬 종료
            if (Input.anyKeyDown)
            {
                StopBlockDash();
            }

            return; // 나머지 일반 이동/점프 로직은 실행 안 함
        }


        // 1. 인공 중력 적용
        rb.AddForce(Vector3.down * calculatedGravity, ForceMode.Acceleration);

        // 2. 바닥 체크
        CheckGrounded();

        // 착지했으면 수평속도 배수 원래대로 복구
        if (isGrounded)
        {
            horizontalSpeedMultiplier = 1f;
        }

        // 3. 입력 처리
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight   = cameraTransform.right;
        camForward.y = 0f; camRight.y = 0f;
        camForward.Normalize(); camRight.Normalize();

        Vector3 inputDir        = (camForward * v + camRight * h).normalized;
        Vector3 currentVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (inputDir.magnitude > 0.1f)
        {
            // 목표 속도 = 기본 속도 * 점프패드 배수
            Vector3 targetVelocity = inputDir * targetMoveSpeed * horizontalSpeedMultiplier;

            Vector3 newVelocity = Vector3.Lerp(
                currentVelocity,
                targetVelocity,
                Time.fixedDeltaTime * acceleration
            );

            rb.linearVelocity = new Vector3(newVelocity.x, rb.linearVelocity.y, newVelocity.z);
        }
        else
        {
            // 입력 없을 때는 착지 상태에서만 강하게 브레이크
            if (isGrounded)
            {
                Vector3 slowedVelocity = Vector3.Lerp(
                    currentVelocity,
                    Vector3.zero,
                    Time.fixedDeltaTime * stopFriction * 5f
                );

                rb.linearVelocity = new Vector3(slowedVelocity.x, rb.linearVelocity.y, slowedVelocity.z);
            }
        }
    }

    void CheckGrounded()
    {
        // 레이를 조금 길게 쏴서 착지 순간을 안정적으로 포착
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.7f);
    }

    void OnCollisionEnter(Collision collision)
    {
        // ★ 대쉬 중에 어떤 물체와 부딪히면 대쉬 종료
        if (isDashByBlock)
        {
            StopBlockDash();
        }

        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.9f)
            {
                float heightMul   = 1f; // 기본 바닥
                float distanceMul = 1f; // 기본 거리

                // 점프패드라면 배수 값을 가져온다
                TileJumpPad pad = collision.collider.GetComponent<TileJumpPad>();
                if (pad != null)
                {
                    heightMul   = pad.heightMultiplier;   // 예: 2
                    distanceMul = pad.distanceMultiplier; // 예: 5/3
                }

                DoJump(heightMul, distanceMul);
                break;
            }
        }
    }

    /// <summary>
    /// heightMultiplier: 기본 점프 높이 배수 (2 → 높이 2배)
    /// distanceMultiplier: 기본 점프 거리 배수 (5/3 → 거리 3칸 → 5칸)
    /// </summary>
    public void DoJump(float heightMultiplier, float distanceMultiplier)
    {
        // 1. 높이 n배 → 초기 수직 속도는 √n 배
        float hMul = Mathf.Max(1f, heightMultiplier);
        float yVel = jumpVelocity * Mathf.Sqrt(hMul);

        // 2. distanceMultiplier 에 맞추기 위해
        //    수평속도 배수 = (원하는 거리배수) / (체공시간 배수(=√n))
        float distMul = Mathf.Max(1f, distanceMultiplier);
        float baseTimeScale = Mathf.Sqrt(hMul);               // 공중 시간 배수
        horizontalSpeedMultiplier = distMul / baseTimeScale;  // 수평 속도 보정

        // 3. 착지 직후 브레이크 로직 유지
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 vel = rb.linearVelocity;

        if (h == 0 && v == 0 && isGrounded)
        {
            vel.x *= 0.5f;
            vel.z *= 0.5f;
        }

        // 4. 최종 수직 속도 적용
        vel.y = yVel;
        rb.linearVelocity = vel;
    }

    // 대쉬블록에서 호출할 함수
    public void StartBlockDash(Vector3 dir, float speed)
    {
        isDashByBlock = true;
        dashDir = dir.normalized;
        dashSpeed = speed;

        // 처음 진입 시 즉시 해당 방향으로 쏴준다
        rb.linearVelocity = dashDir * dashSpeed;
    }

    public void StopBlockDash()
    {
        if (!isDashByBlock) return;

        isDashByBlock = false;

        // 수평 속도는 잠깐 멈추고, 다음 FixedUpdate부터 다시 인공 중력 적용
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
    }
}
