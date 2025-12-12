using UnityEngine;

public class StarMovement : MonoBehaviour
{
    public float rotateSpeed = 100f;
    public float floatSpeed = 2f;
    public float floatHeight = 0.25f;

    private Vector3 startPos;
    private MeshRenderer meshRenderer;
    private Collider col;
    private bool isCollected = false;

    bool IsInStage()
        => GameManager.instance != null && GameManager.instance.Mode == GameManager.GameMode.InStage;

    void Awake()
    {
        startPos = transform.position;
        meshRenderer = GetComponent<MeshRenderer>();
        col = GetComponent<Collider>();

        PlayerRespawn.OnReset += ResetStar;
    }

    void OnDestroy()
    {
        PlayerRespawn.OnReset -= ResetStar;
    }

    void Update()
    {
        if (isCollected) return;

        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        // ✅ StageSelect에서는 먹히면 안 됨
        if (!IsInStage()) return;
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            isCollected = true;

            GameManager.instance.GetStar();

            meshRenderer.enabled = false;
            col.enabled = false;
        }
    }

    void ResetStar()
    {
        // ✅ StageSelect에서 죽을 때 “모든 별 등장” 버그 차단
        if (!IsInStage()) return;

        isCollected = false;
        meshRenderer.enabled = true;
        col.enabled = true;
    }
}
