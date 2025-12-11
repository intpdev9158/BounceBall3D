using UnityEngine;
using UnityEngine.SceneManagement; 

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("스테이지 정보")]
    public int totalStars = 0;      
    public int collectedStars = 0;  

    void Awake()
    {
        // 1. 싱글톤 패턴 (중복 방지)
        if (instance == null)
        {
            instance = this;
            
            // ⭐ 핵심: 씬이 바뀌어도 나를 파괴하지 마라!
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            // 만약 이미 매니저가 있는데 또 하나가 생기면?
            // (예: 1탄에서 만든 매니저가 있는데, 2탄 씬 파일에 또 매니저가 들어있는 경우)
            // 2탄에 있던 '짝퉁' 매니저는 스스로 사라집니다.
            Destroy(gameObject);
        }
    }

    // ⭐ 중요: 매니저가 안 죽기 때문에 Start() 대신 이 기능을 써야 합니다.
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        // 플레이어 리셋 신호 구독
        PlayerRespawn.OnReset += ResetScore;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        // 구독 해지
        PlayerRespawn.OnReset -= ResetScore;
    }

    // 리셋 신호가 오면 점수만 0으로!
    void ResetScore()
    {
        collectedStars = 0;
        Debug.Log("사망! 점수 초기화됨.");
    }

    // 씬 로딩이 끝나면 호출되는 함수
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {   
        // ⭐ 여기가 핵심 수정 포인트! ⭐
        // "내가 진짜 매니저(instance)가 아니라면, 아무것도 하지 말고 꺼져라!"
        if (this != instance) return;

        // 여기서 별 갯수를 초기화하고 다시 셉니다.
        collectedStars = 0;
        totalStars = GameObject.FindGameObjectsWithTag("Star").Length;

        Debug.Log(scene.name + " 도착! 별 갯수 재설정 완료: " + totalStars + "개");
    }

    public void GetStar()
    {
        collectedStars++;
        Debug.Log("별 획득! (" + collectedStars + " / " + totalStars + ")");

        if (collectedStars >= totalStars)
        {
            StageClear();
        }
    }

    void StageClear()
    {
        Debug.Log("스테이지 클리어!");
        
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("🏆 게임 전체 클리어!");
        }
    }


}