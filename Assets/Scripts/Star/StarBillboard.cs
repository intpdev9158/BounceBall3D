using UnityEngine;

public class StarBillboard : MonoBehaviour
{
    void LateUpdate()
    {
        var cam = Camera.main;
        if (!cam) return;

        // 카메라를 향해 항상 정면을 보게(HP바 스타일)
        transform.LookAt(transform.position + cam.transform.rotation * Vector3.forward,
                         cam.transform.rotation * Vector3.up);
    }
}
