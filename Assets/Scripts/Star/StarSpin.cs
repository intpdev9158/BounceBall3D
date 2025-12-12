using UnityEngine;

public class StarSpin : MonoBehaviour
{
    [Header("Rotation")]
    public float rotateSpeed = 120f;

    [Header("Floating")]
    public bool useFloat = true;
    public float floatSpeed = 2f;
    public float floatHeight = 0.25f;

    Vector3 startPos;

    void Awake()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // 회전
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime, Space.World);

        // 둥둥(선택)
        if (useFloat)
        {
            float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(startPos.x, newY, startPos.z);
        }
    }
}
