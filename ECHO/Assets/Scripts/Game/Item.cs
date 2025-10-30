// Item.cs
using UnityEngine;
using TMPro; // (선택) 아이템 근처에 'E'키 UI를 띄우려면

public class Item : MonoBehaviour
{
    public string itemName = "CardKey"; // GameManager가 식별할 이름
    public string popupMessage = "카드키를 획득했다!"; // 획득 시 팝업에 뜰 메시지

    // (선택) E키를 누르라는 UI 텍스트
    public GameObject interactionUI; 

    private bool playerIsNear = false;

    void Start()
    {
        // 시작할 때 UI 숨기기
        if (interactionUI != null)
            interactionUI.SetActive(false);
            
        // 아이템에 콜라이더가 없으면 에러 발생
        if (GetComponent<Collider2D>() == null)
            Debug.LogError(name + " 아이템에 Collider2D가 없습니다!");
        // 콜라이더가 IsTrigger가 아니면 에러 발생
        if (!GetComponent<Collider2D>().isTrigger)
            Debug.LogWarning(name + " 아이템의 Collider2D가 IsTrigger가 아닙니다.");
    }

    void Update()
    {
        // 플레이어가 근처에 있고 E키를 눌렀다면
        if (playerIsNear && Input.GetKeyDown(KeyCode.E))
        {
            // 1. GameManager에게 "아이템 획득" 신호 보내기
            GameManager.Instance.OnItemCollected(itemName, popupMessage);
            
            // 2. (선택) UI가 켜져 있었다면 끄기
            if (interactionUI != null)
                interactionUI.SetActive(false);

            // 3. 아이템 오브젝트 자신을 파괴 (또는 SetActive(false))
            Destroy(gameObject); 
        }
    }

    // 플레이어가 콜라이더 범위에 들어왔을 때
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNear = true;
            // (선택) "E키" UI 켜기
            if (interactionUI != null)
                interactionUI.SetActive(true);
        }
    }

    // 플레이어가 콜라이더 범위에서 나갔을 때
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNear = false;
            // (선택) "E키" UI 끄기
            if (interactionUI != null)
                interactionUI.SetActive(false);
        }
    }
}