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
            // 1. GameData에 Stage 4 (인덱스 3)로 돌아가라고 "예약"
            // (주석과 달리, 이 코드가 *반드시* 필요합니다!)
            // GameData.StageToReload = 3; 
            
            // 2. Core 씬을 로드
            // SceneManager.LoadScene("Core");

            // 3. Additive 방식 함수는 호출하지 않음
            // GameManager.Instance.ReturnToStage4FromEnding(); // (X)

            // GameManager가 아닌, SceneManager로 "Credit" 씬을 직접 로드
            SceneManager.LoadScene("Credit");
        }
    }
}