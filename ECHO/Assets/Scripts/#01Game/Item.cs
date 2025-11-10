// Item.cs
using UnityEngine;
using TMPro; // (선택) 아이템 근처에 'E'키 UI를 띄우려면

// [RequireComponent(typeof(AudioSource))] // AudioSource가 자동으로 추가되도록 함
public class Item : MonoBehaviour
{
    public string itemName = "CardKey"; // GameManager가 식별할 이름
    public string popupMessage = "카드키를 획득했다!"; // 획득 시 팝업에 뜰 메시지

    // (선택) E키를 누르라는 UI 텍스트
    public GameObject interactionUI;
    public AudioClip collectSound; // 인스펙터에서 획득 효과음 연결
    private AudioSource audioSource; // 소리를 재생할 컴포넌트

    private bool playerIsNear = false;

    void Start()
    {
        // AudioSource 컴포넌트를 찾아서 변수에 저장
        audioSource = GetComponent<AudioSource>();

        // 2D 환경에서는 3D 사운드가 필요 없으므로 2D로 설정
        audioSource.spatialBlend = 0f;

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
        // 소리 재생 (GameManager로 넘어가기 전에 재생)
        if (collectSound != null)
        {
            // PlayOneShot은 동시에 여러 소리가 나도 겹치지 않게 재생
            // (주의: Destroy(gameObject)보다 먼저 호출되어야 함)
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }

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