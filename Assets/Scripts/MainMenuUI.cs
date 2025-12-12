using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Play 누르면 이동할 씬 이름")]
    [SerializeField] private string firstStageSceneName = "Stage1"; // 네 첫 스테이지 씬 이름으로

    void Start()
    {
        // 메인메뉴는 클릭해야 하니까 커서 풀기
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void OnClickPlay()
    {
        SceneManager.LoadScene(firstStageSceneName);
    }

    public void OnClickExit()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
