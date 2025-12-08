using UnityEngine;

public class TileSpike : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {   
            foreach (ContactPoint contact in collision.contacts)
            {   
                // [수정됨] 로그에 -1.0(아래쪽)이 찍히므로, -0.7보다 작으면(더 아래면) 윗면 충돌로 인정
                // 즉, 공이 위에서 아래로(-1) 힘을 가했다는 뜻입니다.
                if (contact.normal.y < -0.7f)
                {   
                    PlayerRespawn playerScript = collision.gameObject.GetComponent<PlayerRespawn>();
                    
                    if (playerScript != null)
                    {
                        playerScript.Respawn();
                    }
                    break;
                }
            }
        }
    }
}