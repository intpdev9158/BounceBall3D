using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class StagePortalStar : MonoBehaviour
{
    [Header("Stage Info")]
    public WorldId world;
    public int stageIndex;              // 0=Stage1, 1=Stage2 ...
    public string stageSceneName;       // 로드할 씬 이름

    [Header("Visuals")]
    public GameObject openVisual;       // 노란 별
    public GameObject lockedVisual;     // 회색 별

    [Header("Label (공용 1개)")]
    public TMP_Text labelText;          // Label/TMP_Text 하나만 연결
    public string lockedText = "Unlock"; // 잠김일 때 표시 텍스트

    Collider triggerCol;

    void Awake()
    {
        // 이 스크립트가 OpenStar에 붙어있다는 가정:
        triggerCol = GetComponent<Collider>();
    }

    void Start()
    {
        RefreshVisual(); // Start에서 확정(Dev override 적용 이후)
    }

    public void RefreshVisual()
    {
        bool unlocked = (StageProgress.I == null) ? true : StageProgress.I.IsUnlocked(world , stageIndex);

        if (openVisual) openVisual.SetActive(unlocked);
        if (lockedVisual) lockedVisual.SetActive(!unlocked);

        // 잠김이면 입장 불가
        if (triggerCol) triggerCol.enabled = unlocked;

        // ✅ 라벨은 하나만: 상태에 따라 텍스트 변경
        if (labelText)
        {
            labelText.text = unlocked ? $"STAGE {stageIndex + 1}" : lockedText;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        bool unlocked = (StageProgress.I == null) ? true : StageProgress.I.IsUnlocked(world , stageIndex);
        if (!unlocked) return;

        GameManager.instance.EnterStage(world, stageIndex, stageSceneName);
    }
}
