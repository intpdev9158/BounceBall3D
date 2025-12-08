using UnityEngine;

public class TileBreak : MonoBehaviour
{

    void Awake()
    {
        PlayerRespawn.OnRespawn += ResetTile;
    }

    void Oestroy()
    {
        PlayerRespawn.OnRespawn -= ResetTile;
    }

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
                    HideTile();
                    break;
                }
            }
        }
    }

    void HideTile()
    {   
        // [나중에 할 일] 여기에 이펙트 생성 코드를 넣으면 됩니다.
        // 예: Instantiate(effectPrefab, transform.position, Quaternion.identity);

        gameObject.SetActive(false);   
    }

    void ResetTile()
    {
        CancelInvoke();
        gameObject.SetActive(true);
    }
}