using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class BlobShadow : MonoBehaviour
{
    [Header("Raycast")]
    public Transform target;                 // 공(플레이어)
    public LayerMask groundMask;             // 블록 레이어만 지정
    public float raycastStartOffset = 1.0f;  // 공 중심에서 위로 살짝 쏴서 안정성
    public float raycastMaxDistance = 50f;

    [Header("Shadow Look")]
    public float surfaceOffset = 0.02f;      // 바닥에 살짝 띄우기(z-fighting 방지)
    public float minScale = 0.25f;           // 공이 높이 떴을 때(작게)
    public float maxScale = 1.10f;           // 공이 바닥에 가까울 때(크게)
    public float fadeStart = 0.5f;           // 이 거리부터 페이드 시작
    public float fadeEnd = 6.0f;             // 이 거리에서 거의 사라짐
    public float maxAlpha = 0.55f;           // 바닥 가까울 때 진하기
    public float minAlpha = 0.00f;           // 멀 때 알파

    [Header("Optional: auto generate soft circle texture")]
    public bool autoGenerateTexture = true;
    [Range(32, 512)] public int textureSize = 256;

    MeshRenderer mr;
    MaterialPropertyBlock mpb;

    static readonly int ColorID = Shader.PropertyToID("_Color");       // Built-in
    static readonly int BaseColorID = Shader.PropertyToID("_BaseColor"); // URP

    void Awake()
    {
        mr = GetComponent<MeshRenderer>();
        mpb = new MaterialPropertyBlock();

        if (autoGenerateTexture)
        {
            var tex = MakeSoftCircle(textureSize);
            // 머티리얼 인스턴스 안 만들고도 가능하지만, 가장 간단히 sharedMaterial에 넣어도 OK
            if (mr.sharedMaterial != null)
                mr.sharedMaterial.mainTexture = tex;
        }

        if (target == null)
            Debug.LogWarning("[BlobShadow] target(공)이 비어있어요.");
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 origin = target.position + Vector3.up * raycastStartOffset;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, raycastMaxDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            // 1) 위치: 블록 표면
            transform.position = hit.point + hit.normal * surfaceOffset;

            // 2) 바닥 노멀에 맞춰 살짝 기울게(원하면 고정도 가능)
            transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Quad가 바닥을 보도록(필요하면 값 바꿈)

            // 3) 거리 기반으로 크기/알파 조절
            float dist = (origin.y - hit.point.y);
            float tScale = Mathf.InverseLerp(0f, fadeEnd, dist); // 0(가까움) ~ 1(멀어짐)
            float scale = Mathf.Lerp(maxScale, minScale, tScale);

            float tFade = Mathf.InverseLerp(fadeStart, fadeEnd, dist);
            float alpha = Mathf.Lerp(maxAlpha, minAlpha, tFade);

            transform.localScale = new Vector3(scale, scale, 1f);

            // 4) 알파 적용(URP/Built-in 둘 다 커버)
            mr.GetPropertyBlock(mpb);
            Color c = Color.black;
            c.a = alpha;
            mpb.SetColor(ColorID, c);
            mpb.SetColor(BaseColorID, c);
            mr.SetPropertyBlock(mpb);

            if (!mr.enabled) mr.enabled = true;
        }
        else
        {
            // 아래에 블록이 없으면 그림자 숨김
            if (mr.enabled) mr.enabled = false;
        }
    }

    Texture2D MakeSoftCircle(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.45f; // 원 크기
        float feather = size * 0.18f; // 가장자리 부드러움

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float d = Vector2.Distance(new Vector2(x, y), center);
            float a = 1f - Mathf.InverseLerp(radius - feather, radius, d);
            a = Mathf.Clamp01(a);
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, a)); // 흰색 + 알파 (머티리얼 색으로 검정 처리)
        }

        tex.Apply();
        return tex;
    }
}
