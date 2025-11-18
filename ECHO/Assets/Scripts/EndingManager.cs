using UnityEngine;
// SceneManager는 이제 GameManager가 처리하므로 필요 없지만, 비상용으로 남겨둡니다.
using UnityEngine.SceneManagement; 

public class EndingScenario : MonoBehaviour 
{

    // Finish 버튼의 OnClick 이벤트에 연결할 함수
    public void OnFinishButtonClicked()
    {
        Debug.Log("크레딧으로 이동합니다.");

        if (GameManager.Instance != null)
        {
            // GameManager에게 다음 스테이지(Credit)로 넘겨달라고 요청함
            // (아까 NextStage 함수를 고쳤으므로, Ending -> Credit으로 잘 넘어감)
            GameManager.Instance.NextStage();
        }
        else
        {
            // 비상용: GameManager가 없을 경우 직접 로드
            SceneManager.LoadScene("Credit");
        }
    }
}