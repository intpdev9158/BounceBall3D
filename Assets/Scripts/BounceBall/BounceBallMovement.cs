using UnityEngine;

public class BounceBallMovement : MonoBehaviour
{
    [Header("Master Control")]
    public float bouncesPerSecond = 1.5f;
    public float jumpHeight = 3.0f;

    [Header("Movement Control")]
    public float maxDistance = 3.0f;
    public float acceleration = 5.0f;
    public float stopFriction = 5.0f;

    public Transform cameraTransform;

    private Rigidbody rb;
    private bool isGrounded;

    // Physics 계산값
    private float calculatedGravity;
    private float jumpVelocity;
    private float targetMoveSpeed;

    // 점프패드 수평 배수
    private float horizontalSpeedMultiplier = 1f;

    [Header("Dash Control")]
    public float dashSpeed = 5f;
    private bool isDashByBlock = false;
    private Vector3 dashDir;

    // =========================
    // External Boost (AirBoost XZ 성분)
    // =========================
    [Header("External Boost (XZ)")]
    public float externalBoostMaxSpeed = 30f;
    private Vector3 externalBoostXZ = Vector3.zero;

    // =========================
    // AirBoost Stop (WASD 눌러서 "부스트만" 끊기)
    // =========================
    [Header("AirBoost Cancel")]
    [Tooltip("AirBoost 발동 직후 더블탭 입력과 충돌 방지용")]
    [SerializeField] private float cancelIgnoreTimeAfterBoost = 0.06f;

    private float lastAirBoostTime = -999f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.freezeRotation = true;
        rb.useGravity = false;
        rb.linearDamping = 0f;

        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        SyncPhysics();
    }

    void OnValidate()
    {
        if (rb != null) SyncPhysics();
    }

    void SyncPhysics()
    {
        float timeInAir = 1.0f / bouncesPerSecond;
        float timeToApex = timeInAir / 2.0f;

        calculatedGravity = (2f * jumpHeight) / (timeToApex * timeToApex);
        jumpVelocity = calculatedGravity * timeToApex;

        targetMoveSpeed = maxDistance / timeInAir;
    }

    void Update()
    {
        // ✅ 공중 + 부스트가 남아있을 때,
        // ✅ WASD를 누르면 "부스트만 끊고" 그 이후 공중컨트롤은 그대로 살린다.
        if (isDashByBlock) return;

        bool groundedNow = Physics.Raycast(transform.position, Vector3.down, 0.7f);
        if (groundedNow) return;

        // 부스트 직후(더블탭) 입력은 무시
        if (Time.time - lastAirBoostTime < cancelIgnoreTimeAfterBoost) return;

        if (externalBoostXZ.sqrMagnitude <= 0.0001f) return;

        bool wasdDown =
            Input.GetKeyDown(KeyCode.W) ||
            Input.GetKeyDown(KeyCode.A) ||
            Input.GetKeyDown(KeyCode.S) ||
            Input.GetKeyDown(KeyCode.D);

        if (wasdDown)
        {
            CancelBoostAndKeepControl();
        }
    }

    void FixedUpdate()
    {
        // 대쉬 블록 이동
        if (isDashByBlock)
        {
            rb.linearVelocity = new Vector3(
                dashDir.x * dashSpeed,
                0f,
                dashDir.z * dashSpeed
            );

            if (Input.anyKeyDown)
                StopBlockDash();

            return;
        }

        // 1) 인공 중력
        rb.AddForce(Vector3.down * calculatedGravity, ForceMode.Acceleration);

        // 2) 바닥 체크
        CheckGrounded();

        // 착지하면 수평 배수 초기화 + (원하면) 부스트 성분도 정리
        if (isGrounded)
        {
            horizontalSpeedMultiplier = 1f;

            // 착지 시 부스트가 남아있을 가능성 제거
            if (externalBoostXZ.sqrMagnitude > 0.0001f)
            {
                Vector3 v0 = rb.linearVelocity;
                rb.linearVelocity = new Vector3(v0.x - externalBoostXZ.x, v0.y, v0.z - externalBoostXZ.z);
                externalBoostXZ = Vector3.zero;
            }
        }

        // 3) 입력 처리(카메라 기준)
        float inputH = Input.GetAxisRaw("Horizontal");
        float inputV = Input.GetAxisRaw("Vertical");

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f; camRight.y = 0f;
        camForward.Normalize(); camRight.Normalize();

        Vector3 inputDir = (camForward * inputV + camRight * inputH).normalized;

        // 현재 XZ 속도에서 '부스트 성분'을 제외한 base 속도
        Vector3 rawXZ = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        Vector3 baseXZ = rawXZ - externalBoostXZ;

        Vector3 newBaseXZ = baseXZ;

        if (inputDir.magnitude > 0.1f)
        {
            Vector3 targetVelocity = inputDir * targetMoveSpeed * horizontalSpeedMultiplier;
            newBaseXZ = Vector3.Lerp(baseXZ, targetVelocity, Time.fixedDeltaTime * acceleration);
        }
        else
        {
            if (isGrounded)
                newBaseXZ = Vector3.Lerp(baseXZ, Vector3.zero, Time.fixedDeltaTime * stopFriction * 5f);
        }

        // 최종 XZ = base + boost
        Vector3 finalXZ = newBaseXZ + externalBoostXZ;
        rb.linearVelocity = new Vector3(finalXZ.x, rb.linearVelocity.y, finalXZ.z);
    }

    void CheckGrounded()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.7f);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isDashByBlock) StopBlockDash();

        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.9f)
            {
                float heightMul = 1f;
                float distanceMul = 1f;

                TileJumpPad pad = collision.collider.GetComponent<TileJumpPad>();
                if (pad != null)
                {
                    heightMul = pad.heightMultiplier;
                    distanceMul = pad.distanceMultiplier;
                }

                DoJump(heightMul, distanceMul);
                break;
            }
        }
    }

    public void DoJump(float heightMultiplier, float distanceMultiplier)
    {
        float hMul = Mathf.Max(1f, heightMultiplier);
        float yVel = jumpVelocity * Mathf.Sqrt(hMul);

        float distMul = Mathf.Max(1f, distanceMultiplier);
        float baseTimeScale = Mathf.Sqrt(hMul);
        horizontalSpeedMultiplier = distMul / baseTimeScale;

        Vector3 vel = rb.linearVelocity;
        vel.y = yVel;
        rb.linearVelocity = vel;
    }

    // =========================
    // AirBoost API (Controller가 호출)
    // =========================
    public float GetBaseJumpVelocity() => jumpVelocity;

    /// <summary>
    /// ✅ AirBoost: XZ는 externalBoost로 유지 + Y는 낙하 끊고 다시 위로(재포물선)
    /// </summary>
    public void ApplyAirBoost(Vector3 worldDir, float addXZSpeed, float upVelocity)
    {
        lastAirBoostTime = Time.time;

        AddExternalBoostXZ(worldDir, addXZSpeed);

        Vector3 vel = rb.linearVelocity;
        if (vel.y < 0f) vel.y = 0f;
        vel.y = Mathf.Max(vel.y, upVelocity);
        rb.linearVelocity = vel;
    }

    public void ApplyUpBoost(Vector3 worldDir, float upVelocityMultiplier)
    {
        // 외부 XZ 부스트가 남아있으면 제거(능력 충돌 방지)
        if (externalBoostXZ.sqrMagnitude > 0.0001f)
        {
            Vector3 v0 = rb.linearVelocity;
            rb.linearVelocity = new Vector3(v0.x - externalBoostXZ.x, v0.y, v0.z - externalBoostXZ.z);
            externalBoostXZ = Vector3.zero;
        }

        Vector3 vel = rb.linearVelocity;

        // 낙하 중이면 낙하를 끊고
        if (vel.y < 0f) vel.y = 0f;

        // 바닥 점프급으로 다시 위로
        float upVel = jumpVelocity * Mathf.Max(0.01f, upVelocityMultiplier);
        vel.y = Mathf.Max(vel.y, upVel);

        // 현재 WASD 방향으로 “점프 다시 그리는 느낌”
        worldDir.y = 0f;
        if (worldDir.sqrMagnitude > 0.001f)
        {
            worldDir.Normalize();
            float speed = targetMoveSpeed * horizontalSpeedMultiplier;
            vel.x = worldDir.x * speed;
            vel.z = worldDir.z * speed;
        }

        rb.linearVelocity = vel;
    }


    /// <summary>
    /// ✅ 외부부스트(XZ)를 base 이동과 분리해서 관리
    /// </summary>
    public void AddExternalBoostXZ(Vector3 worldDir, float addSpeed)
    {
        worldDir.y = 0f;
        if (worldDir.sqrMagnitude < 0.0001f) return;

        Vector3 before = externalBoostXZ;
        Vector3 after = before + worldDir.normalized * addSpeed;

        float max = Mathf.Max(0.1f, externalBoostMaxSpeed);
        if (after.magnitude > max)
            after = after.normalized * max;

        Vector3 delta = after - before;
        externalBoostXZ = after;

        // 즉시 체감되게 현재 속도에도 반영
        rb.linearVelocity += new Vector3(delta.x, 0f, delta.z);
    }

    /// <summary>
    /// ✅ WASD 누르면 "부스트만" 끊기 (수직낙하 고정 X)
    /// - 부스트 성분 제거
    /// - 현재 XZ를 0으로 한 번 정리(멈추는 느낌)
    /// - 그 다음 프레임부터는 입력대로 공중 컨트롤 가능
    /// </summary>
    void CancelBoostAndKeepControl()
    {
        // 현재 XZ 멈춤(정지 포인트 만들기)
        Vector3 vel = rb.linearVelocity;
        rb.linearVelocity = new Vector3(0f, vel.y, 0f);

        // 부스트 성분 제거
        externalBoostXZ = Vector3.zero;

        // ✅ "키를 누르고 있으면 떨어지면서 그 방향으로 가야함"을 즉시 반영하고 싶다면:
        // (키다운 순간에 이미 방향키를 누르고 있으면 바로 그 방향으로 XZ를 부여)
        float inputH = Input.GetAxisRaw("Horizontal");
        float inputV = Input.GetAxisRaw("Vertical");

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f; camRight.y = 0f;
        camForward.Normalize(); camRight.Normalize();

        Vector3 inputDir = (camForward * inputV + camRight * inputH);
        inputDir.y = 0f;

        if (inputDir.sqrMagnitude > 0.001f)
        {
            inputDir.Normalize();
            Vector3 now = rb.linearVelocity;
            Vector3 xz = inputDir * targetMoveSpeed * horizontalSpeedMultiplier;
            rb.linearVelocity = new Vector3(xz.x, now.y, xz.z);
        }
    }

    // =========================
    // Dash Block API
    // =========================
    public void StartBlockDash(Vector3 dir, float speed)
    {
        externalBoostXZ = Vector3.zero;
        isDashByBlock = true;
        dashDir = dir.normalized;
        dashSpeed = speed;

        rb.linearVelocity = dashDir * dashSpeed;
    }

    public void StopBlockDash()
    {
        if (!isDashByBlock) return;

        isDashByBlock = false;
        externalBoostXZ = Vector3.zero;

        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
    }
}
