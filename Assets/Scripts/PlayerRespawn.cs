using UnityEngine;
using System; // 이벤트(Action)를 쓰기 위해 필요

public class PlayerRespawn : MonoBehaviour
{
    public float fallThreshold = -10.0f;
    private Vector3 startPosition;
    private Rigidbody rb;

    // "나 부활한다!"라고 방송하는 확성기
    public static event Action OnReset; 

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
    }

    void Update()
    {
        if (transform.position.y < fallThreshold)
        {
            Respawn();
        }
    }

    public void Respawn()
    {
        // 1. 플레이어 물리력 초기화 (가속도 제거)
        rb.linearVelocity = Vector3.zero; // Unity 6 이하라면 rb.velocity 사용
        rb.angularVelocity = Vector3.zero;
        
        // 2. 위치 원상복구 (텔레포트)
        transform.position = startPosition;

        // 3. 방송하기: "모든 별과 타일은 원위치로!"
        OnReset?.Invoke();
    }
}