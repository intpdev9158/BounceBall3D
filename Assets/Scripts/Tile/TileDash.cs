using UnityEngine;
using UnityEngine.Diagnostics;

public class TileDash : MonoBehaviour
{   
    [Header("대쉬 설정")]
    public Vector3 localDashDirection = Vector3.right; // 화살표가 가리키는 방향
    public float dashSpeed = 5f;

    [Tooltip("블록 중심에서 대쉬 방향으로 얼마나 빼줄지")]
    public float startOffset = 0.6f;   // 네가 말한 0.6

    // 플레이어가 "위에서" 들어올 때만 발동시키기
    void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag("Player"))
            return;

        // 접촉 면들 순회하면서, 플레이어가 위에서 내려왔는지 체크
        bool fromTop = false;
        foreach (var c in collision.contacts)
        {
            // contact.normal = 블록 기준 법선
            if (c.normal.y < -0.7f)
            {
                fromTop = true;
                break;
            }
        }

        if (!fromTop) return;

        // 실제 대쉬 시작
        BounceBallMovement player = collision.collider.GetComponent<BounceBallMovement>();
        if (player != null)
        {   
            // ★ 여기서 블록 정중앙으로 스냅
            var col = GetComponent<Collider>();
            Vector3 center = col.bounds.center;
            // 대쉬 방향(월드 기준)
            Vector3 dir = transform.TransformDirection(localDashDirection).normalized;

            // ★ 시작 위치 계산
            //   - x,z 는 블록 중심에서 dir 방향으로 startOffset만큼 이동
            //   - y 는 블록 센터 그대로 사용
            Vector3 spawnPos = center + dir * startOffset;
            spawnPos.y = center.y;

            player.transform.position = spawnPos;

            player.StartBlockDash(dir, dashSpeed);
        }
    }
}
