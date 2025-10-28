using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // --- Singleton Pattern ---
    // 씬이 변경되어도 GameManager가 플레이어/스테이지를 관리할 수 있도록 싱글톤으로 만듭니다.
    public static GameManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Core 씬 전체가 유지된다면 이 코드는 필요 없을 수 있습니다.
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // -------------------------

    public int totalPoint;
    public int stagePoint;
    public int stageIndex;
    public int health;
    public PlayerMove player;
    
    // GameObject 배열 대신 씬 이름 리스트로 변경
    public List<string> stageSceneNames = new List<string>();
    
    // 현재 스테이지의 정보를 저장할 변수
    private Stage currentStage; 

    public Image[] UIhealth;
    public TMP_Text UIPoint;
    public TMP_Text UIStage;
    public GameObject UIRestartBtn;
    public Camera mainCamera; // 줌인할 메인 카메라
    public string cutsceneSceneName; // 로드할 컷씬 씬의 이름
    public float deathZoomDuration; // 줌인에 걸리는 시간 (초)
    public float targetZoomSize;    // 줌인 목표 크기 (숫자가 작을수록 줌인)
    public Sprite fullHeartSprite;  // 인스펙터에서 '찬 하트' 이미지를 연결할 변수
    public Sprite emptyHeartSprite; // 인스펙터에서 '빈 하트' 이미지를 연결할 변수

    private bool isDead = false;

    public GameObject inactivityPopup;  // 인스펙터에서 연결할 팝업 UI
    public float inactivityLimit = 180f;    // 제한 시간 (초 단위)
    public float popupDuration = 5f;    // 팝업이 뜬 후 대기할 시간 (초 단위)
    private float inactivityTimer = 0f; // 내부 타이머 변수
    private bool isInactive = false;    // 팝업이 떴는지 확인하는 변수

    // [아이템 획득 팝업 UI 변수 추가]
    public GameObject itemPopupPanel; // 획득! 창 (Panel)
    public TMP_Text itemPopupText;  // 획득! 창의 텍스트

    // [미션 UI 변수 추가]
    public TMP_Text UIMissionText;  // 오른쪽 상단 미션 텍스트

    // [미션 진행도 변수 추가]
    private int cardKeysCollected = 0;
    private int cardKeysNeeded = 3; // Stage 4에서 필요한 카드키 수
    private string stage4Mission_InProgress = "카드키를 획득하여 기억보관장치를 가동시키자.";
    private string stage4Mission_Complete = "카드키를 모두 얻었다. 이제 기억보관장치를 가동시켜보자.";

    void Start()
    {
        // 자리 비움 타이머 및 팝업 초기화
        if (inactivityPopup != null)
            inactivityPopup.SetActive(false);
        isInactive = false;
        inactivityTimer = 0f;
        Time.timeScale = 1f; // (혹시 모르니 시간 흐름 복구)

        // [추가] UI 초기화
        if (itemPopupPanel != null)
            itemPopupPanel.SetActive(false); // 아이템 팝업 숨기기
        if (UIMissionText != null)
            UIMissionText.text = ""; // 미션 텍스트 비우기

        // "GameData.cs"에 저장된 스테이지 인덱스를 가져옴
        stageIndex = GameData.StageToReload;

        // 게임 시작 시 첫 스테이지 UI 텍스트 설정
        UIStage.text = "STAGE " + (stageIndex + 1);

        // 저장된 스테이지를 로드하는 코루틴 시작
        StartCoroutine(LoadInitialStage());
    }

    IEnumerator LoadInitialStage()
    {
        // 1. stageIndex에 맞는 씬 이름을 가져옴
        string sceneToLoad = stageSceneNames[stageIndex];
        
        // 2. 해당 씬을 추가로 로드
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
        {
            yield return null; // 로드 완료까지 대기
        }

        // 3. 새로 로드된 씬의 Stage.cs가
        // GameManager에게 등록할 때까지 잠시 기다림
        float waitTimer = 0f;
        while (currentStage == null)
        {
            if (waitTimer > 3.0f) // 3초 이상 등록이 안 되면 에러
            {
                Debug.LogError(sceneToLoad + " 씬에 Stage.cs가 없거나 등록에 실패했습니다!");
                yield break; // 코루틴 중단
            }
            waitTimer += Time.deltaTime;
            yield return null;
        }
        
        // 4. Stage.cs 등록 완료! 플레이어를 스폰 지점으로 이동
        Debug.Log(sceneToLoad + " 로드 완료 및 Stage 등록 완료.");
        PlayerReposition();

        // 스테이지에 맞는 미션 설정
        SetupMissionForStage(stageIndex);
    }

    void Update()
    {
        UIPoint.text = (totalPoint + stagePoint).ToString();

        // [자리 비움 감지 로직 추가]
        // 1. 이미 팝업이 떴다면 타이머를 실행하지 않음
        if (isInactive)
        {
            return;
        }

        // 2. 키보드, 마우스 등 '아무' 입력이라도 감지되면
        if (Input.anyKeyDown || 
            Input.GetAxisRaw("Horizontal") != 0 || 
            Input.GetAxisRaw("Vertical") != 0 ||
            Input.GetAxis("Mouse X") != 0 ||
            Input.GetAxis("Mouse Y") != 0)
        {
            // 3. 타이머를 리셋
            inactivityTimer = 0f;
        }
        else
        {
            // 4. 아무 입력이 없으면 타이머 시간 증가
            inactivityTimer += Time.deltaTime;
        }

        // 5. 타이머가 제한 시간을 초과하면
        if (inactivityTimer >= inactivityLimit)
        {
            // 6. "비활성" 상태로 만들고 팝업 코루틴을 실행 (한 번만)
            isInactive = true;
            StartCoroutine(ReturnToMainMenuRoutine());
        }
    }

    // Stage.cs가 호출할 메서드
    public void RegisterStage(Stage stage)
    {
        currentStage = stage;
        Debug.Log(stage.name + " 스테이지가 등록되었습니다.");
    }

    // NextStage 로직을 코루틴으로 분리
    public void NextStage()
    {
        // Change Stage
        if (stageIndex < stageSceneNames.Count - 1)
        {
            StartCoroutine(NextStageRoutine());
        }
        else    // Game Clear
        {
            // Player Control Lock
            Time.timeScale = 0;

            // Result UI
            Debug.Log("게임 클리어!");

            // Restart Button UI
            // Text -> TMP_Text
            TMP_Text btnText = UIRestartBtn.GetComponentInChildren<TMP_Text>();
            if (btnText != null)
                btnText.text = "Clear!";
            UIRestartBtn.SetActive(true);
        }
    }

    // 씬을 비동기(Async)로 로드/언로드하는 코루틴
    IEnumerator NextStageRoutine()
    {
        // 1. 현재 스테이지 씬을 언로드
        if (!string.IsNullOrEmpty(stageSceneNames[stageIndex]))
        {
            SceneManager.UnloadSceneAsync(stageSceneNames[stageIndex]);
        }

        // 2. 인덱스 및 포인트 계산
        stageIndex++;
        totalPoint += stagePoint;
        stagePoint = 0;

        // 3. 다음 스테이지 씬을 Additive로 로드하고 완료될 때까지 대기
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(stageSceneNames[stageIndex], LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
        {
            yield return null; // 씬 로드가 끝날 때까지 1프레임 대기
        }

        // 4. 새 씬이 로드 완료됨 (이때쯤 새 씬의 Stage.cs가 Start/Awake에서 RegisterStage를 호출했을 것임)

        // 5. UI 텍스트 업데이트
        UIStage.text = "STAGE " + (stageIndex + 1);

        // 6. 플레이어 재배치
        PlayerReposition();
    }

    public void HealthDown()
    {

        if (isDead) return;

        if (health > 1)
        {
            health--;
            // UIhealth[health].color = new Color(1, 0, 0, 0.4f);
            UIhealth[health].sprite = emptyHeartSprite;
        }
        else
        {
            // "죽음" 상태로 변경
            isDead = true;
            health = 0;

            // All Health UI Off
            // UIhealth[0].color = new Color(1, 0, 0, 0.4f);
            UIhealth[0].sprite = emptyHeartSprite;

            // Player Die Effect
            player.OnDie();

            // Result UI
            Debug.Log("죽었습니다.");

            // "GameData.cs"에 현재 스테이지 인덱스 저장
            GameData.StageToReload = stageIndex;

            // Retry Button UI
            // UIRestartBtn.SetActive(true);
            StartCoroutine(PlayerDeathSequence());
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            // Player Reposition
            if (health > 1)
            {
                PlayerReposition(); // 이제 현재 스테이지의 스폰 지점으로 이동
            }

            // Health Down
            HealthDown();
        }
    }

    // 현재 스테이지의 스폰 지점을 사용
    void PlayerReposition()
    {
        if (currentStage != null && currentStage.spawnPoint != null)
        {
            // 등록된 현재 스테이지의 스폰 지점으로 플레이어 이동
            player.transform.position = currentStage.spawnPoint.position;
        }
        else
        {
            // 혹시 모르니 예외 처리 (기존 코드)
            player.transform.position = new Vector3(0, 0, -1);
            Debug.LogWarning("Current Stage 또는 Spawn Point가 등록되지 않아 기본 위치로 스폰합니다.");
        }
        
        player.VelocityZero();
    }

    public void Restart()
    {
        Time.timeScale = 1;

        // 처음부터 다시 시작하는 것이므로, 리로드할 스테이지를 0으로 리셋
        GameData.StageToReload = 0;

        SceneManager.LoadScene("Core"); // Core씬 로드
    }

    public void RetryStage()
    {
        isDead = false;

        // 1. 시간 다시 흐르게
        Time.timeScale = 1;

        // 2. 리트라이 버튼 숨기기
        UIRestartBtn.SetActive(false);

        // 3. 체력 변수 초기화 (최대 체력값으로)
        health = 3; // (최대 체력이 3이라고 가정)

        // 4. 체력 UI 초기화 (전부 꽉 찬 색으로)
        for (int i = 0; i < UIhealth.Length; i++)
        {
            // (꽉 찬 하트의 원래 색상으로 변경, 예: 흰색)
            // UIhealth[i].color = new Color(1, 1, 1, 1);
            UIhealth[i].sprite = fullHeartSprite;
        }

        // 5. 플레이어 스크립트에 "부활" 신호 보내기
        player.Respawn(); // PlayerMove.cs에 Respawn()이 있어야 함

        // 6. 현재 스테이지의 스폰포인트로 플레이어 이동
        PlayerReposition();
    }

    // 플레이어 사망 연출 코루틴
    IEnumerator PlayerDeathSequence()
    {
        // 1. Die 애니메이션이 재생될 시간을 잠시 기다림
        yield return new WaitForSeconds(0.5f); // 0.5초 대기 (Die 애니메이션 길이에 맞춰 조절)

        // 2. 줌인 목표 지점(플레이어 머리)과 시작 값 설정
        // (카메라의 z축 위치는 그대로 유지해야 함)
        Vector3 targetPosition = new Vector3(
            player.headTransform.position.x,
            player.headTransform.position.y,
            mainCamera.transform.position.z
        );
        Vector3 startPosition = mainCamera.transform.position;
        float startZoomSize = mainCamera.orthographicSize;

        float timer = 0f;

        // 3. 줌인 루프 (정해진 시간(deathZoomDuration) 동안 실행)
        while (timer < deathZoomDuration)
        {
            // 시간을 0~1 사이의 비율로 변환 (부드러운 이동을 위해 SmoothStep 사용)
            float t = timer / deathZoomDuration;
            float smoothT = t * t * (3f - 2f * t); // SmoothStep

            // 카메라 위치와 줌(orthographicSize)을 부드럽게 변경
            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, smoothT);
            mainCamera.orthographicSize = Mathf.Lerp(startZoomSize, targetZoomSize, smoothT);

            timer += Time.deltaTime;
            yield return null; // 다음 프레임까지 대기
        }

        // 4. (선택) 줌인 완료 후 잠시 멈춰서 보여주기
        yield return new WaitForSeconds(1.0f); // 1초간 줌인 상태 유지

        // 5. 컷씬 씬 로드
        SceneManager.LoadScene(cutsceneSceneName);
    }

    IEnumerator ReturnToMainMenuRoutine()
    {
        Debug.Log("자리 비움 감지됨. 팝업을 띄웁니다.");

        // 1. 팝업 UI를 켬
        if (inactivityPopup != null)
            inactivityPopup.SetActive(true);

        // 2. 팝업이 떠 있는 시간(popupDuration)만큼 대기
        yield return new WaitForSeconds(popupDuration);

        Debug.Log("메인 메뉴로 돌아갑니다.");

        // 3. (필수) 시간 정지 상태일 수 있으니 1로 복구
        Time.timeScale = 1f;

        // 4. MainMenu 씬을 로드
        SceneManager.LoadScene("MainMenu");
    }
    
    void SetupMissionForStage(int index)
    {
        // Stage 4의 인덱스가 3이라고 가정 (0,1,2,3)
        if (index == 3) 
        {
            cardKeysCollected = 0; // 스테이지 시작 시 카드키 초기화
            cardKeysNeeded = 3;    // 필요한 카드키 수 설정
            UIMissionText.text = stage4Mission_InProgress; // 미션 텍스트 설정
        }
        else
        {
            // 다른 스테이지는 미션 없음
            UIMissionText.text = ""; 
        }
    }
    // [새로 추가] Item.cs가 이 함수를 호출
    public void OnItemCollected(string itemName, string message)
    {
        // 1. 팝업 코루틴 실행
        StartCoroutine(ShowItemPopup(message));

        // 2. 미션 진행도 업데이트
        if (itemName == "CardKey")
        {
            cardKeysCollected++;
            CheckMissionProgress(); // 미션 상태 확인
        }
        // (else if (itemName == "HealthPotion") { ... } 등등)
    }

    // [새로 추가] 아이템 팝업을 2초간 띄웠다가 숨기는 코루틴
    IEnumerator ShowItemPopup(string message)
    {
        itemPopupText.text = message;
        itemPopupPanel.SetActive(true);

        yield return new WaitForSeconds(2.0f); // 2초 대기

        itemPopupPanel.SetActive(false);
    }

    // [새로 추가] 미션이 완료되었는지 확인하는 함수
    void CheckMissionProgress()
    {
        // 현재 스테이지가 Stage 4 (인덱스 3)일 때만 확인
        if (stageIndex == 3)
        {
            if (cardKeysCollected >= cardKeysNeeded)
            {
                // 카드키를 다 모았다면
                UIMissionText.text = stage4Mission_Complete;
            }
            else
            {
                // 아직 덜 모았다면 (이건 요청엔 없었지만 더 친절한 UI)
                UIMissionText.text = "카드키 (" + cardKeysCollected + "/" + cardKeysNeeded + ") 획득.";
                // 원래 요청대로 하려면 이 else문을 지우세요.
            }
        }
    }
}