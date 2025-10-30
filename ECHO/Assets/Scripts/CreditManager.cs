using System.Collections; // 코루틴을 사용하기 위해 추가
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CreditManager : MonoBehaviour
{
    // --- 이미지/패널 변수: 역할을 명확히 하도록 변수명 변경 및 데이터 타입 조정 ---

    // 1. 마무리 멘트 (5초) - Image 컴포넌트 또는 Panel의 GameObject
    [SerializeField]
    private GameObject finishMessagePanel; 
    
    // 2. [엔딩] E.C.H.O. (5초) - Image 컴포넌트 또는 Panel의 GameObject
    [SerializeField]
    private GameObject endingImagePanel; 
    
    // 3. [크레딧] Credit 패널 (5초) - Panel GameObject
    [SerializeField]
    private GameObject creditPanel; 

    // 4. 마지막 패널 (목록 버튼, 메인화면 버튼, 게임종료 버튼을 포함할 패널)
    [SerializeField]
    private GameObject LastPanel; // 마지막에 활성화할 패널

    // 이전 버튼 관련 변수들은 제거됨 (listButton, mainMenuButton, exitGameButton)


    void Start()
    {
        // 1. 모든 UI를 시작할 때 비활성화합니다.
        SetAllUIActive(false);

        // 2. 순차적으로 UI를 활성화하는 코루틴을 시작합니다.
        StartCoroutine(CreditSequenceRoutine());
    }

    /// <summary>
    /// 모든 UI 요소를 일괄적으로 활성화/비활성화합니다.
    /// </summary>
    private void SetAllUIActive(bool isActive)
    {
        if (finishMessagePanel != null) finishMessagePanel.SetActive(isActive);
        if (endingImagePanel != null) endingImagePanel.SetActive(isActive);
        if (creditPanel != null) creditPanel.SetActive(isActive);
        
        // 🌟 LastPanel의 활성화 상태를 제어합니다.
        if (LastPanel != null) LastPanel.SetActive(isActive); 
    }


    /// <summary>
    /// 요청하신 순서와 시간 간격에 따라 패널을 활성화합니다.
    /// </summary>
    IEnumerator CreditSequenceRoutine()
    {
        float waitTime = 5f; // 기본 대기 시간

        // 1. '마무리 멘트' 활성화 (5초)
        if (finishMessagePanel != null)
        {
            finishMessagePanel.SetActive(true);
            yield return new WaitForSeconds(waitTime);
            finishMessagePanel.SetActive(false); // 다음 패널로 전환을 위해 비활성화
        }
        
        // 2. '엔딩 (E.C.H.O.)' 활성화 (5초)
        if (endingImagePanel != null)
        {
            endingImagePanel.SetActive(true);
            yield return new WaitForSeconds(waitTime);
            endingImagePanel.SetActive(false); // 다음 패널로 전환을 위해 비활성화
        }

        // 3. '크레딧' 패널 활성화 (5초)
        if (creditPanel != null)
        {
            creditPanel.SetActive(true);
            yield return new WaitForSeconds(waitTime);
            creditPanel.SetActive(false);
        }
        
        // 🌟 4. 'LastPanel' 활성화 (버튼들이 포함된 최종 패널)
        if (LastPanel != null)
        {
            // 크레딧 패널 활성화 후 5초 대기 시점에 마지막 패널 활성화
            LastPanel.SetActive(true);
        }
    }


    // --- 버튼에 연결할 공용 함수 (LastPanel 내의 버튼에 연결) ---

    // 메인메뉴로 돌아가는 함수
    public void ReturnToMainMenu()
    {
        // "MainMenu" 씬을 로드합니다.
        SceneManager.LoadScene("#00MainMenu");
    }

    // 게임 종료 함수
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // 목록 버튼 함수 (기능은 임시로 추가)
    public void OpenListPanel()
    {
        Debug.Log("목록 버튼 클릭: '지금까지 남겨진 마음들' 목록 화면으로 이동합니다.");
        // SceneManager.LoadScene("ListScene"); // 실제 씬 전환 로직 추가 필요
    }
}