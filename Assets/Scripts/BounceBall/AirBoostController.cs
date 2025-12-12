using UnityEngine;

public class AirBoostController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private BounceBallMovement mover;
    [SerializeField] private Renderer ballRenderer;

    [Header("AirBoost (XZ + Y)")]
    [SerializeField] private float boostAddXZSpeed = 15f;

    [Tooltip("원작처럼 '다시 포물선' 만들기 위한 위로 속도 = (기본 점프속도 * 배수)")]
    [SerializeField] private float upVelocityMultiplier = 0.75f;

    [SerializeField] private Color boostColor = Color.black;

    [Header("Double Tap")]
    [SerializeField] private float doubleTapWindow = 0.25f;

    [Header("Air Check")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundCheckDistance = 0.7f;
    [SerializeField] private float minAirHeightToUse = 0.15f;

    private bool active = false;

    private float lastTapW = -999f, lastTapA = -999f, lastTapS = -999f, lastTapD = -999f;

    private Color originalColor = Color.yellow;
    private bool originalColorSaved = false;

    static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    static readonly int ColorID = Shader.PropertyToID("_Color");
    private MaterialPropertyBlock mpb;

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
            ResetTapTimes();
            return;
        }

        if (GetHeightToGround() <= minAirHeightToUse) return;

        if (TryDoubleTap(KeyCode.W, ref lastTapW)) { UseAirBoost(GetDirW()); return; }
        if (TryDoubleTap(KeyCode.A, ref lastTapA)) { UseAirBoost(GetDirA()); return; }
        if (TryDoubleTap(KeyCode.S, ref lastTapS)) { UseAirBoost(GetDirS()); return; }
        if (TryDoubleTap(KeyCode.D, ref lastTapD)) { UseAirBoost(GetDirD()); return; }
    }

    public void SetEnabled(bool enabled)
    {
        active = enabled;
        SaveOriginalColorOnce();
        ApplyColor();
        ResetTapTimes();
    }

    void UseAirBoost(Vector3 dir)
    {
        if (!mover) return;

        float upVel = mover.GetBaseJumpVelocity() * upVelocityMultiplier;

        // ✅ 핵심: XZ + Y(재포물선)
        mover.ApplyAirBoost(dir, boostAddXZSpeed, upVel);

        // 1회 사용 후 종료 + 노란색 복귀
        active = false;
        ApplyColor();
        ResetTapTimes();

        // 능력 관리자 있으면 None으로 내리기(선택)
        var ability = GetComponent<AbilityController>();
        if (ability) ability.SetAbility(AbilityType.None);
    }

    bool TryDoubleTap(KeyCode key, ref float lastTapTime)
    {
        if (!Input.GetKeyDown(key)) return false;

        float now = Time.time;
        bool isDouble = (now - lastTapTime) <= doubleTapWindow;
        lastTapTime = now;
        return isDouble;
    }

    void ResetTapTimes()
    {
        lastTapW = lastTapA = lastTapS = lastTapD = -999f;
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

    // 카메라 기준 방향
    Vector3 GetFlatForward()
    {
        var cam = Camera.main;
        Vector3 f = cam ? cam.transform.forward : transform.forward;
        f.y = 0f;
        return f.sqrMagnitude > 0.001f ? f.normalized : Vector3.forward;
    }

    Vector3 GetFlatRight()
    {
        var cam = Camera.main;
        Vector3 r = cam ? cam.transform.right : transform.right;
        r.y = 0f;
        return r.sqrMagnitude > 0.001f ? r.normalized : Vector3.right;
    }

    Vector3 GetDirW() => GetFlatForward();
    Vector3 GetDirS() => -GetFlatForward();
    Vector3 GetDirD() => GetFlatRight();
    Vector3 GetDirA() => -GetFlatRight();
}
