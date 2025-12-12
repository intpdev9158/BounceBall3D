using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public enum GameMode
    {
        StageSelect,
        InStage
    }

    [Header("현재 모드")]
    public GameMode Mode { get; private set; } = GameMode.StageSelect;

    [Header("스테이지 별 정보 (스테이지 씬에서만 사용)")]
    public int totalStars = 0;
    public int collectedStars = 0;

    [Header("씬 이름")]
    [SerializeField] private string stageSelectSceneName = "StageSelect";

    // ✅ 현재 들어온 스테이지 인덱스(포탈에서 설정)
    public WorldId CurrentWorld { get; private set; } = WorldId.UnderGround;
    public int CurrentStageIndex { get; private set; } = -1;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayerRespawn.OnReset += ResetScore;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        PlayerRespawn.OnReset -= ResetScore;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (this != instance) return;

        // ✅ 항상 인게임처럼
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Mode = (scene.name == stageSelectSceneName) ? GameMode.StageSelect : GameMode.InStage;

        if (Mode == GameMode.InStage)
        {
            collectedStars = 0;
            totalStars = GameObject.FindGameObjectsWithTag("Star").Length;
        }
        else
        {
            collectedStars = 0;
            totalStars = 0;

            // 스테이지 선택 씬으로 돌아오면 스테이지 인덱스는 의미 없으니 초기화(선택)
            CurrentStageIndex = -1;
        }
    }

    void ResetScore()
    {
        if (Mode != GameMode.InStage) return;
        collectedStars = 0;
        Debug.Log("사망! 점수 초기화됨.");
    }

    /// <summary>
    /// ✅ StageSelect 포탈 별에서 호출:
    /// 입장할 스테이지 인덱스를 저장하고 씬 로드
    /// </summary>
    public void EnterStage(WorldId world, int stageIndex, string stageSceneName)
{
    CurrentWorld = world;
    CurrentStageIndex = stageIndex;
    SceneManager.LoadScene(stageSceneName);
}

    /// <summary>
    /// '스테이지 씬'에서 먹는 별만 이 함수를 호출해야 함.
    /// (StageSelect 포탈 별은 이거 호출하면 안 됨)
    /// </summary>
    public void GetStar()
    {
        if (Mode != GameMode.InStage) return;

        collectedStars++;
        Debug.Log($"별 획득! ({collectedStars} / {totalStars})");

        if (totalStars <= 0) return;

        if (collectedStars >= totalStars)
        {
            StageClear();
        }
    }

    void StageClear()
    {
        // ✅ 1순위: 포탈에서 저장해둔 CurrentStageIndex로 저장
        if (StageProgress.I != null && CurrentStageIndex >= 0)
        {
            StageProgress.I.MarkCleared(CurrentWorld, CurrentStageIndex);
        }
        else
        {
            // ✅ 2순위(보험): StageInfo가 있으면 그걸로 저장 (기존 방식 유지)
            StageInfo info = FindFirstObjectByType<StageInfo>();
            if (info != null && StageProgress.I != null)
                StageProgress.I.MarkCleared(CurrentWorld, CurrentStageIndex);
        }

        SceneManager.LoadScene(stageSelectSceneName);
    }
}
