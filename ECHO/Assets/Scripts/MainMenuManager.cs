using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void StartGame()
    {
        // 1. GameData에 '처음부터'(0번 스테이지) 시작하도록 설정
        GameData.StageToReload = 0;
        
        // 2. Intro 씬 로드
        SceneManager.LoadScene("Intro");
    }

    // (선택) 게임 종료 버튼
    // public void QuitGame()
    // {
    //     Application.Quit();
    // }
}