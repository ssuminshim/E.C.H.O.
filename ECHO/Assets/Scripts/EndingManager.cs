using UnityEngine;
using UnityEngine.SceneManagement;

// 스크립트의 클래스 이름은 실제 파일 이름과 일치해야 합니다.
public class EndingScenario : MonoBehaviour 
{
    // [ ★★★ 수정 1 ★★★ ]
    // Start() 함수에 씬을 로드하는 코루틴(WaitForSeconds 등)이 있다면,
    // 그 Start() 함수를 통째로 삭제하세요!
    // (Start() 함수는 비워둡니다)
    void Start()
    {
        // 3초 뒤 씬 전환하는 코드가 있다면 모두 삭제
        // (예: StartCoroutine(LoadSceneAfterTime(3.0f)); <-- 삭제!)
    }

    // Update 함수만 남겨두고, 스페이스바를 누를 때만 작동하게 합니다.
    void Update()
    {
        // 엔딩 연출이 끝나고 (또는 아무때나) 스페이스바를 누르면
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // [ ★★★ 수정 2: 가장 중요 ★★★ ]
            // GameData.StageToReload 값을 *절대* 건드리지 않습니다!
            
            // GameData.StageToReload = 0; // <--- 이런 코드가 있다면 반드시 삭제하세요!
            
            // Core 씬을 로드합니다.
            // GameManager가 알아서 GameData에 저장된 '3'을 읽을 것입니다.
            SceneManager.LoadScene("Core");
        }
    }
}