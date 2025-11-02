using UnityEngine;
using TMPro;

public class Machine : MonoBehaviour
{
    // E키 상호작용을 보여줄 UI (선택 사항, 머신 오브젝트의 자식으로 두는 것이 일반적입니다)
    public GameObject interactionUI; 

    private bool playerIsNear = false;
    // private bool interactionAllowed = false; // GameManager에서 실시간으로 받아오므로 이 변수는 불필요해짐

    void Start()
    {
        // 시작할 때 UI 숨기기
        if (interactionUI != null)
            interactionUI.SetActive(false);
            
        // 콜라이더가 IsTrigger인지 확인
        Collider2D coll = GetComponent<Collider2D>();
        if (coll == null)
            Debug.LogError(name + " 머신에 Collider2D가 없습니다!");
        if (coll != null && !coll.isTrigger)
            Debug.LogWarning(name + " 머신의 Collider2D가 IsTrigger가 아닙니다. 상호작용을 위해 IsTrigger를 켜야 합니다.");
    }

    void Update()
    {
        // 플레이어가 근처에 있을 때만 로직 실행
        if (playerIsNear)
        {
            // 1. 카드키 3개를 모두 모았는지 GameManager를 통해 실시간으로 확인
            bool interactionAllowed = GameManager.Instance.IsCardKeyMissionComplete();

            // 2. 상호작용이 가능하고 E키를 눌렀다면
            if (interactionAllowed && Input.GetKeyDown(KeyCode.E))
            {
                InteractWithMachine();
            }

            // 3. UI 상태 업데이트
            // 상호작용이 허용될 때만 UI를 켜고, 그렇지 않으면 끕니다.
            if (interactionUI != null)
            {
                // 현재 UI 활성화 상태와 필요한 활성화 상태가 다를 때만 변경
                if (interactionUI.activeSelf != interactionAllowed)
                {
                    interactionUI.SetActive(interactionAllowed);
                }
            }
        }
    }

    void InteractWithMachine()
    {
        // 1. GameManager의 최종 패널 활성화 함수 호출
        GameManager.Instance.ActivateCompletionPanel();
        
        // 2. 상호작용이 완료되었으므로 이 스크립트 비활성화
        this.enabled = false; 
        
        // 3. UI도 비활성화
        if (interactionUI != null)
            interactionUI.SetActive(false);
    }

    // 플레이어가 콜라이더 범위에 들어왔을 때
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNear = true;
            // 미션 완료 상태라면 바로 UI를 켜줍니다. (Update에서 실시간 체크하므로 필수 아님)
            // if (interactionUI != null && GameManager.Instance.IsCardKeyMissionComplete())
            //     interactionUI.SetActive(true);
        }
    }

    // 플레이어가 콜라이더 범위에서 나갔을 때
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNear = false;
            // "E키" UI 끄기
            if (interactionUI != null)
                interactionUI.SetActive(false);
        }
    }
}