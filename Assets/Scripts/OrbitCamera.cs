using Unity.Collections;
using UnityEngine;

public class OrbitCamera : MonoBehaviour
{

    [Header("Target Setting")]
    public Transform target;
    public float distance = 5.0f;
    public float height = 1.0f;

    [Header("Mouse Setting")]
    public float xSpeed = 200.0f;
    public float ySpeed = 100.0f;

    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    public float xRotation = 0.0f;
    public float yRotation = 0.0f;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        xRotation = angles.y;
        yRotation = angles.x;

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (target)
        {
            xRotation += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            yRotation -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

            yRotation = ClampAngle(yRotation , yMinLimit , yMaxLimit);

            Quaternion rotation = Quaternion.Euler(yRotation, xRotation ,0);

            Vector3 position = rotation * new Vector3(0.0f , 0.0f, -distance) + target.position + new Vector3(0, height, 0);

            transform.rotation = rotation;
            transform.position = position;
        }
    }

    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f) angle += 360f;
        if (angle > 360f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }
}
