using UnityEngine;

public class Machine : MonoBehaviour
{
    private bool isPlayerInRange = false; // 플레이어가 범위 안에 있는지
    private bool isActivated = false;

    // (선택사항) "E키를 누르세요" 안내 UI가 있다면 여기에 연결
    public GameObject interactionGuideUI; 

    void Start()
    {
        if (interactionGuideUI != null) interactionGuideUI.SetActive(false);
    }

    void Update()
    {
        // 1. 플레이어가 범위 안에 있고 + E키를 눌렀을 때
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    void TryInteract()
    {
        // 이미 기억을 봤다면 무시
        if (GameData.HasCompletedMemory) return;
        
        // 이미 활성화(작동) 중이라면 중복 실행 방지
        if (isActivated) return;

        // GameManager에게 "기계 열어줘" 요청
        // (카드키 검사는 GameManager가 수행하도록 위임하거나 여기서 해도 됨. 
        //  여기서는 깔끔하게 GameManager의 새로운 함수를 호출합니다.)
        GameManager.Instance.TryOpenMachinePanel();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            if (interactionGuideUI != null && !GameData.HasCompletedMemory) 
                interactionGuideUI.SetActive(true); // 안내 UI 켜기
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (interactionGuideUI != null) 
                interactionGuideUI.SetActive(false); // 안내 UI 끄기
        }
    }
}