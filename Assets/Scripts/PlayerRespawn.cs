using System;
using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [Header("Settings")]
    public float fallThreshold = -30.0f;

    private Vector3 startPosition;
    private Rigidbody rb;

    public static event Action OnRespawn;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // 게임 시작 시점의 공 위치를 '부활 지점'으로 기억
        startPosition = transform.position;
    }


    void Update()
    {
        if(transform.position.y < fallThreshold)
        {
            Respawn();
        }
    }

    public void Respawn()
    {
        transform.position = startPosition;

        // 떨어지던 회전, 가속도 제거
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 부활 이벤트 호출
        OnRespawn?.Invoke();
    }
}
