using UnityEngine;

public class TileAbilityPickup : MonoBehaviour
{
    [Header("Grant")]
    [SerializeField] private AbilityType grantAbility = AbilityType.AirBoost;

    [Header("Visual (Quad 묶음 Root)")]
    [SerializeField] private GameObject visualRoot;   // 자식 Tile_XXX(Quad들 묶음)

    [Header("Trigger")]
    [SerializeField] private Collider triggerCol;     // 루트 Trigger

    private bool consumed;

    void Awake()
    {
        if (!triggerCol) triggerCol = GetComponent<Collider>();

        // visualRoot 비워두면: 첫 번째 자식을 VisualRoot로 가정(너 프리팹 구조에 딱 맞음)
        if (!visualRoot && transform.childCount > 0)
            visualRoot = transform.GetChild(0).gameObject;

        PlayerRespawn.OnReset += ResetTile;
    }

    void OnDestroy()
    {
        PlayerRespawn.OnReset -= ResetTile;
    }

    void OnTriggerEnter(Collider other)
    {
        if (consumed) return;

        // ✅ 콜라이더가 자식일 수 있으니 attachedRigidbody 우선
        GameObject playerGO = other.attachedRigidbody ? other.attachedRigidbody.gameObject : other.gameObject;
        if (!playerGO.CompareTag("Player")) return;

        var ability = playerGO.GetComponent<AbilityController>();
        if (ability != null)
        {
            ability.SetAbility(grantAbility); // ✅ 덮어쓰기
        }
        else
        {
            Debug.LogWarning($"[TileAbilityPickup] Player에 AbilityController가 없음: {playerGO.name}");
        }

        Consume();
    }

    void Consume()
    {
        consumed = true;
        if (visualRoot) visualRoot.SetActive(false);
        if (triggerCol) triggerCol.enabled = false;
    }

    void ResetTile()
    {
        consumed = false;
        if (visualRoot) visualRoot.SetActive(true);
        if (triggerCol) triggerCol.enabled = true;
    }
}
