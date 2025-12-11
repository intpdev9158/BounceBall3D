using UnityEngine;

public class TileJumpPad : MonoBehaviour
{
    [Header("점프 높이 배수 (기본 = 1)")]
    [Tooltip("2로 하면 기본 점프 높이의 2배만큼 뜀")]
    public float heightMultiplier = 2f;      // 높이 2배

    [Header("점프 거리 배수 (기본 = 1)")]
    [Tooltip("기본 거리가 3칸일 때, 5/3로 하면 약 5칸까지 날아감")]
    public float distanceMultiplier = 5f / 3f; // 3칸 → 5칸
}
