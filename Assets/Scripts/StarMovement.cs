using UnityEngine;

public class StarMovement : MonoBehaviour
{
    [Header("운동 설정")]
    public float rotateSpeed = 100f; 
    public float floatSpeed = 2f;    
    public float floatHeight = 0.25f; 
    private Vector3 startPos;

    // 내 몸통(그림)과 껍데기(충돌)
    private MeshRenderer meshRenderer;
    private Collider col;
    private bool isCollected = false; 

    void Awake()
    {
        startPos = transform.position;
        meshRenderer = GetComponent<MeshRenderer>();
        col = GetComponent<Collider>();

        // 플레이어의 "Reset" 신호를 구독(듣기)합니다.
        PlayerRespawn.OnReset += ResetStar;
    }

    void OnDestroy()
    {
        // 내가 진짜 사라질 때는 구독 해지 (에러 방지)
        PlayerRespawn.OnReset -= ResetStar;
    }

    void Update()
    {
        if (isCollected) return; // 먹힌 상태면 움직이지 않음

        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            isCollected = true; 
            
            // 점수 올리기
            GameManager.instance.GetStar();

            // ⭐ Destroy 대신 '숨기기' 모드 발동
            meshRenderer.enabled = false; // 눈에 안 보임
            col.enabled = false;          // 만져지지 않음
        }
    }

    // "Reset" 신호를 들으면 실행되는 함수
    void ResetStar()
    {
        isCollected = false;
        meshRenderer.enabled = true; // 다시 보임
        col.enabled = true;          // 다시 만져짐
    }
}