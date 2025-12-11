using UnityEngine;

public class TileBreak : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private Collider col;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        col = GetComponent<Collider>();
        
        // 리셋 신호 구독
        PlayerRespawn.OnReset += ResetTile;
    }

    void OnDestroy()
    {
        PlayerRespawn.OnReset -= ResetTile;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            foreach (ContactPoint contact in collision.contacts)
            {
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
        // ⭐ Destroy나 SetActive(false) 대신 컴포넌트만 끄기
        meshRenderer.enabled = false; 
        col.enabled = false;
    }

    void ResetTile()
    {
        meshRenderer.enabled = true;
        col.enabled = true;
    }
}