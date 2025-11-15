using UnityEngine;
using UnityEngine.SceneManagement;

// 스크립트의 클래스 이름은 실제 파일 이름과 일치해야 함
public class EndingScenario : MonoBehaviour 
{
    // Start() 함수는 비워둠
    void Start()
    {
        
    }

    // Update 함수만 남겨두고, 스페이스바를 누를 때만 작동하게 함
    void Update()
    {
        // 엔딩 연출이 끝나고 (또는 아무때나) 스페이스바를 누르면
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // GameData.StageToReload 값 *절대* 건드리지 XXXX!!
            // Core 씬을 로드
            SceneManager.LoadScene("Core");
        }
    }
}