using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // TMP_Text를 사용한다면 필요

public class PreviousLifeManager : MonoBehaviour
{
    // 인스펙터 창에서 "스페이스바를 눌러..." 텍스트 오브젝트를 연결
    public GameObject pressSpaceText; 
    
    // (선택) 대화 UI 오브젝트
    // public GameObject dialogueUI; 

    private bool canRetry = false; // 스페이스바를 누를 수 있는 상태인가?

    void Start()
    {
        pressSpaceText.SetActive(false);
        canRetry = false;
        StartCoroutine(DialogueSequence());
    }

    IEnumerator DialogueSequence()
    {
        // ------------------------------------------
        // 여기에 씬의 대화/컷씬 연출 로직
        // ------------------------------------------
        
        Debug.Log("회상씬 대화가 재생 중입니다...");
        yield return new WaitForSeconds(5.0f); // (임시) 5초간 대화가 재생된다고 가정
        Debug.Log("회상씬 대화가 끝났습니다.");

        // ------------------------------------------
        // 대화가 끝나면...
        // ------------------------------------------

        // "재도전" 텍스트를 킴
        pressSpaceText.SetActive(true);
        canRetry = true;
    }

    void Update()
    {
        // "재도전"이 활성화됐고, 스페이스바를 눌렀다면
        if (canRetry && Input.GetKeyDown(KeyCode.Space))
        {
            // Core 씬을 로드
            // Core 씬의 GameManager는 GameData.StageToReload 값을 읽어서
            // 우리가 죽었던 그 스테이지를 알아서 로드
            SceneManager.LoadScene("Core");
        }
    }
}