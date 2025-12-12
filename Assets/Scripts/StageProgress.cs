using UnityEngine;
using System;

public class StageProgress : MonoBehaviour
{
    public static StageProgress I;

    const string KEY_PREFIX = "HighestClearedStage_"; // ex) HighestClearedStage_UnderGround
    const string OLD_KEY = "HighestClearedStage";     // 예전 단일 키(마이그레이션용)

    [Header("월드별 스테이지 개수(순서: UnderGround, Ground, Sky)")]
    [SerializeField] private int[] worldStageCounts = new int[] { 3, 3, 3 };

    private int[] highestCleared; // 월드별 최고 클리어(0-based), 기본 -1

    [Header("DEV TEST (Editor)")]
    [SerializeField] private bool devOverride = false;
    [SerializeField] private bool devSessionOnly = true;

    // 월드별로 테스트 진행도 입력 (0-based, -1이면 아무것도 안 깬 상태)
    [SerializeField] private int devUGHighest = -1;
    [SerializeField] private int devGHighest = -1;
    [SerializeField] private int devSKYHighest = -1;

    void Awake()
    {
        if (I == null)
        {
            I = this;
            DontDestroyOnLoad(gameObject);

            int worldCount = Enum.GetValues(typeof(WorldId)).Length;
            highestCleared = new int[worldCount];

            // 기본 -1
            for (int i = 0; i < worldCount; i++) highestCleared[i] = -1;

            // 저장 로드
            foreach (WorldId w in Enum.GetValues(typeof(WorldId)))
            {
                string key = KEY_PREFIX + w.ToString();
                highestCleared[(int)w] = PlayerPrefs.GetInt(key, -1);
            }

            // ✅ 예전 단일 저장값이 있으면 UnderGround로 마이그레이션(한 번만)
            if (PlayerPrefs.HasKey(OLD_KEY))
            {
                int old = PlayerPrefs.GetInt(OLD_KEY, -1);
                if (highestCleared[(int)WorldId.UnderGround] < old)
                {
                    highestCleared[(int)WorldId.UnderGround] = old;
                    PlayerPrefs.SetInt(KEY_PREFIX + WorldId.UnderGround, old);
                    PlayerPrefs.Save();
                }
                PlayerPrefs.DeleteKey(OLD_KEY);
            }
            ApplyDevOverride();
        }
        else Destroy(gameObject);
    }

    int GetWorldStageCount(WorldId w)
    {
        int idx = (int)w;
        if (worldStageCounts == null || idx < 0 || idx >= worldStageCounts.Length)
            return 9999; // 설정 안 했으면 무한처럼 취급
        return worldStageCounts[idx];
    }

    // ✅ 월드 해금 조건: 이전 월드를 "마지막 스테이지까지" 깨야 다음 월드 오픈
    public bool IsWorldUnlocked(WorldId w)
    {
        if (w == WorldId.UnderGround) return true;

        WorldId prev = (WorldId)((int)w - 1);
        int prevNeed = GetWorldStageCount(prev) - 1;       // 이전 월드 마지막 인덱스
        int prevCleared = highestCleared[(int)prev];
        return prevCleared >= prevNeed;
    }

    // ✅ 스테이지 해금: 해당 월드가 열려있고, (클리어+1)까지
    public bool IsUnlocked(WorldId w, int stageIndex)
    {
        if (!IsWorldUnlocked(w)) return false;
        return stageIndex <= highestCleared[(int)w] + 1;
    }

    public void MarkCleared(WorldId w, int stageIndex)
    {
        int wi = (int)w;
        if (stageIndex > highestCleared[wi])
        {
            highestCleared[wi] = stageIndex;
            PlayerPrefs.SetInt(KEY_PREFIX + w.ToString(), highestCleared[wi]);
            PlayerPrefs.Save();
        }
    }

    // 디버그용: 특정 월드만 리셋
    public void ResetWorld(WorldId w)
    {
        highestCleared[(int)w] = -1;
        PlayerPrefs.DeleteKey(KEY_PREFIX + w.ToString());
        PlayerPrefs.Save();
    }

    // 디버그용: 전부 리셋
    public void ResetAll()
    {
        foreach (WorldId w in Enum.GetValues(typeof(WorldId)))
            PlayerPrefs.DeleteKey(KEY_PREFIX + w.ToString());

        PlayerPrefs.Save();

        for (int i = 0; i < highestCleared.Length; i++)
            highestCleared[i] = -1;
    }

    // 개발자용
    void ApplyDevOverride()
    {
        if (!devOverride) return;

        highestCleared[(int)WorldId.UnderGround] = devUGHighest;
        highestCleared[(int)WorldId.Ground]      = devGHighest;
        highestCleared[(int)WorldId.Sky]         = devSKYHighest;

        Debug.Log($"DEV Override 적용: UG={devUGHighest}, G={devGHighest}, SKY={devSKYHighest} (저장={(!devSessionOnly)})");

        // ✅ 세션 전용이 아니면 PlayerPrefs에도 저장
        if (!devSessionOnly)
        {
            PlayerPrefs.SetInt(KEY_PREFIX + WorldId.UnderGround, devUGHighest);
            PlayerPrefs.SetInt(KEY_PREFIX + WorldId.Ground, devGHighest);
            PlayerPrefs.SetInt(KEY_PREFIX + WorldId.Sky, devSKYHighest);
            PlayerPrefs.Save();
        }
    }
}
