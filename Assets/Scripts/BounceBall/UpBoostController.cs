using UnityEngine;

public class UpBoostController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private BounceBallMovement mover;
    [SerializeField] private Renderer ballRenderer;

    [Header("UpBoost")]
    [SerializeField] private float upVelocityMultiplier = 1.0f; // 1.0 = 바닥 점프급
    [SerializeField] private Color boostColor = new Color(0.2f, 0.8f, 1f, 1f);

    [Header("Double Tap (Space)")]
    [SerializeField] private float doubleTapWindow = 0.25f;

    [Header("Air Check")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundCheckDistance = 0.7f;
    [SerializeField] private float minAirHeightToUse = 0.15f;

    bool active = false;
    float lastTapSpace = -999f;

    Color originalColor = Color.yellow;
    bool originalColorSaved = false;

    static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    static readonly int ColorID = Shader.PropertyToID("_Color");
    MaterialPropertyBlock mpb;

    void Awake()
    {
        if (!mover) mover = GetComponent<BounceBallMovement>();
        if (!ballRenderer) ballRenderer = GetComponentInChildren<Renderer>();

        mpb = new MaterialPropertyBlock();
        SaveOriginalColorOnce();
        ApplyColor();
    }

    void Update()
    {
        if (!active) return;

        if (IsGrounded())
        {
            lastTapSpace = -999f;
            return;
        }

        if (GetHeightToGround() <= minAirHeightToUse) return;

        if (!Input.GetKeyDown(KeyCode.Space)) return;

        float now = Time.time;
        bool isDouble = (now - lastTapSpace) <= doubleTapWindow;
        lastTapSpace = now;

        if (!isDouble) return;

        UseUpBoost();
        lastTapSpace = -999f;
    }

    public void SetEnabled(bool enabled)
    {
        active = enabled;
        SaveOriginalColorOnce();
        ApplyColor();
        lastTapSpace = -999f;
    }

    void UseUpBoost()
    {
        if (!mover) return;

        Vector3 dir = ReadMoveDirWorld();
        mover.ApplyUpBoost(dir, upVelocityMultiplier);

        // 1회 사용 후 종료
        active = false;
        ApplyColor();

        var ability = GetComponent<AbilityController>();
        if (ability) ability.SetAbility(AbilityType.None);
    }

    Vector3 ReadMoveDirWorld()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Transform cam = Camera.main ? Camera.main.transform : null;
        if (!cam) return new Vector3(h, 0f, v).normalized;

        Vector3 forward = cam.forward; forward.y = 0f; forward.Normalize();
        Vector3 right   = cam.right;   right.y = 0f;   right.Normalize();

        Vector3 world = forward * v + right * h;
        world.y = 0f;
        return world.sqrMagnitude > 0.001f ? world.normalized : Vector3.zero;
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundMask, QueryTriggerInteraction.Ignore);
    }

    float GetHeightToGround()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out var hit, 50f, groundMask, QueryTriggerInteraction.Ignore))
            return hit.distance;
        return 999f;
    }

    void SaveOriginalColorOnce()
    {
        if (originalColorSaved || !ballRenderer) return;

        var mat = ballRenderer.sharedMaterial;
        if (mat != null)
        {
            if (mat.HasProperty(BaseColorID)) originalColor = mat.GetColor(BaseColorID);
            else if (mat.HasProperty(ColorID)) originalColor = mat.GetColor(ColorID);
        }
        originalColorSaved = true;
    }

    void ApplyColor()
    {
        if (!ballRenderer) return;

        Color c = active ? boostColor : originalColor;

        ballRenderer.GetPropertyBlock(mpb);
        mpb.SetColor(BaseColorID, c);
        mpb.SetColor(ColorID, c);
        ballRenderer.SetPropertyBlock(mpb);
    }
}
