using UnityEngine;

public class BounceBallFinal : MonoBehaviour
{
    [Header("Master Control")]
    public float bouncesPerSecond = 1.5f; 
    public float jumpHeight = 3.0f;       

    [Header("Movement Control")]
    public float maxDistance = 3.0f;      
    public float acceleration = 5.0f;    
    public float stopFriction = 5.0f;    // 값이 클수록 착지 시 빨리 멈춤

    public Transform cameraTransform;
    private Rigidbody rb;
    private bool isGrounded;

    // 내부 변수
    private float calculatedGravity;
    private float jumpVelocity;
    private float targetMoveSpeed; 

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        rb.freezeRotation = true;
        rb.useGravity = false; 
        rb.linearDamping = 0;  

        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        SyncPhysics();
    }

    void OnValidate()
    {
        if(rb != null) SyncPhysics();
    }

    void SyncPhysics()
    {
        float timeInAir = 1.0f / bouncesPerSecond; 
        float timeToApex = timeInAir / 2.0f; 

        calculatedGravity = (2 * jumpHeight) / (timeToApex * timeToApex);
        jumpVelocity = calculatedGravity * timeToApex;
        targetMoveSpeed = maxDistance / timeInAir; 
    }

    void FixedUpdate()
    {
        // 1. 중력 적용
        rb.AddForce(Vector3.down * calculatedGravity, ForceMode.Acceleration);

        // 2. 바닥 체크 (쿨타임 로직 삭제 -> 브레이크를 위해 항상 체크)
        CheckGrounded();

        // 3. 입력 처리
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0; camRight.y = 0;
        camForward.Normalize(); camRight.Normalize();

        Vector3 inputDir = (camForward * v + camRight * h).normalized;
        Vector3 currentVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        if (inputDir.magnitude > 0.1f)
        {
            // [이동 중] 목표 속도로 가속
            Vector3 targetVelocity = inputDir * targetMoveSpeed;
            Vector3 newVelocity = Vector3.Lerp(currentVelocity, targetVelocity, Time.fixedDeltaTime * acceleration);
            rb.linearVelocity = new Vector3(newVelocity.x, rb.linearVelocity.y, newVelocity.z);
        }
        else
        {
            // [키 뗌] 
            // 공중에서는 관성 유지, 땅에 닿으면 즉시 감속
            if (isGrounded)
            {
                // NoFriction 재질 때문에 미끄러우므로, 코드로 강력하게 제동을 걺
                // Lerp 속도를 높여서(Time.fixedDeltaTime * stopFriction * 5) 더 확실하게 멈춤
                Vector3 slowedVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, Time.fixedDeltaTime * stopFriction * 5);
                rb.linearVelocity = new Vector3(slowedVelocity.x, rb.linearVelocity.y, slowedVelocity.z);
            }
        }
    }

    void CheckGrounded()
    {
        // 레이저 길이를 0.7f로 넉넉하게 잡아서 착지 순간을 확실히 포착
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.7f);
    }

    void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.7f)
            {
                // [핵심 기능] 착지 순간 입력이 없으면 수평 속도 팍 깎기 (랜딩 브레이크)
                float h = Input.GetAxisRaw("Horizontal");
                float v = Input.GetAxisRaw("Vertical");
                
                if (h == 0 && v == 0)
                {
                    // 입력이 없으면 X, Z 속도를 절반 이하로 줄여버림 (미끄러짐 방지)
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x * 0.5f, jumpVelocity, rb.linearVelocity.z * 0.5f);
                }
                else
                {
                    // 입력이 있으면 점프만 수행
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpVelocity, rb.linearVelocity.z);
                }
                break;
            }
        }
    }
}