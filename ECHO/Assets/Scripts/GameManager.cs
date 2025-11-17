using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // 씬이 변경되어도 GameManager가 플레이어/스테이지를 관리할 수 있도록 싱글톤으로 만듦
    public static GameManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // 씬이 바뀌어도 GameManager(Player, Camera, UI 포함)가 파괴되지 않게 함
            DontDestroyOnLoad(gameObject); 
            
            // 씬이 로드될 때마다 OnSceneLoaded 함수를 실행하도록 등록
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            // 만약 MainMenu로 돌아왔을 때 이미 GameManager가 존재한다면
            // 새로 생긴 GameManager(나 자신)를 파괴
            Destroy(gameObject);
            return; // Awake()의 나머지 코드를 실행하지 않음
        }

        // GameManager에 붙어있는 AudioSource 컴포넌트를 찾음
        bgmPlayer = GetComponent<AudioSource>();
        if (bgmPlayer != null)
        {
            bgmPlayer.loop = true; // 음악은 항상 반복하도록 설정
        }
    }

    public int stagePoint;
    public int stageIndex;
    public int health;
    public PlayerMove player;

    // 씬이 변경되는 중인지 확인 (중복 호출 방지용)
    private Rigidbody2D playerRigidbody;

    private bool isChangingStage = false;

    // GameObject 배열 대신 씬 이름 리스트로 변경
    public List<string> stageSceneNames = new List<string>();

    // 현재 스테이지의 정보를 저장할 변수
    private Stage currentStage;

    public Image[] UIhealth;
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

    // [아이템 획득 팝업 UI 변수]
    public GameObject itemPopupPanel; // 획득! 창 (Panel)
    public TMP_Text itemPopupText;  // 획득! 창의 텍스트

    // [미션 UI 변수]
    public TMP_Text UIMissionText;  // 오른쪽 상단 미션 텍스트

    // [음악 관리 변수]
    private AudioSource bgmPlayer; // 음악을 재생할 Audio Source
    public AudioClip musicStage1_2; // Stage 1, 2에서 쓸 음악
    public AudioClip musicStage3;   // Stage 3에서 쓸 음악
    public AudioClip musicStage4;   // Stage 4에서 쓸 음악
    public AudioClip musicStage5;   // Stage 5에서 쓸 음악

    [Header("Memory/Ending/Credit BGM")] // (인스펙터에서 보기 쉽게)
    public AudioClip musicMemory;
    public AudioClip musicEnding;
    public AudioClip musicCredit;

    [Header("Walk Sounds")]
    public AudioClip defaultWalkSound; // 기본 걷는 소리
    public AudioClip stage4WalkSound;  // 스테이지 4 전용 걷는 소리

    // [미션 진행도 변수]
    [Header("Mission Settings")] // (인스펙터에서 보기 좋게 제목 추가)
    public string stage4LockedMessage = "우선 기억보관기계를 가동시키자.";
    private int cardKeysCollected = 0;
    private int cardKeysNeeded = 3; // Stage 4에서 필요한 카드키 수
    public string stage4Mission_InProgress = "카드키를 획득하여 기억보관장치를 가동시키자.";
    public string stage4Mission_Complete = "카드키를 모두 얻었다. 이제 기억보관장치를 가동시켜보자.";
    public string stage4Mission_Exit = "문 밖으로 나가보자."; // Memory 씬 이후 미션

    // 머신 상호작용 완료 시 활성화할 패널
    public GameObject machineCompletionPanel;

    // [크레딧 연출용 변수]
    [Header("Credits Settings")]
    public float creditPanDuration = 3.0f; // 카메라가 패닝되는 데 걸리는 시간

    public GameObject gameUIRoot;
    public float defaultCameraSize = 3f;

    void Start()
    {
        // 게임 시작 시 초기화 로직 실행
        InitializeGame();
    }

    // GameManager의 핵심 로직을 초기화합니다.
    // Start()에서 처음 실행되고, Core 씬이 로드될 때마다 다시 실행됩니다.
    void InitializeGame()
    {
        // 1. 카메라 참조를 새로 찾음 ('유령' 참조 방지)
        mainCamera = null; // '유령' 참조를 강제로 null로 만듦
        GameObject cameraObj = GameObject.FindWithTag("MainCamera");
        if (cameraObj != null)
            mainCamera = cameraObj.GetComponent<Camera>();
        else
            Debug.LogError("InitializeGame: 'MainCamera' 태그를 가진 카메라를 찾지 못했습니다!");

        // 2. UI 참조 다시 찾기 (체력 리셋 포함)
        FindAndAssignUI(); 
        
        // 3. 모든 상태 리셋
        isDead = false; 
        isInactive = false;
        inactivityTimer = 0f;
        Time.timeScale = 1f; 

        // 4. 팝업 UI 끄기
        if (inactivityPopup != null)
            inactivityPopup.SetActive(false);
        if (itemPopupPanel != null)
            itemPopupPanel.SetActive(false); 
        if (UIMissionText != null)
            UIMissionText.text = "";
        if (machineCompletionPanel != null)
            machineCompletionPanel.SetActive(false);

        // 5. GameData를 읽어 로드할 스테이지 결정
        if (GameData.StageToReload < 0) 
        {
            stageIndex = 0; 
        }
        else 
        {
            stageIndex = GameData.StageToReload;
        }
        GameData.StageToReload = -1; 

        // 6. UI 텍스트 설정 (FindAndAssignUI 이후에 호출)
        if (UIStage != null) 
            UIStage.text = "STAGE " + (stageIndex + 1);
        else
            Debug.LogError("InitializeGame: UIStage가 null이라 텍스트를 설정할 수 없습니다.");

        // 7. 스테이지 로드 코루틴 실행
        StartCoroutine(LoadInitialStage());
    }

    // 씬이 다시 로드될 때 깨진 UI 참조(Health, Mission 등)를
    // "GameUI" 태그를 기준으로 다시 찾고, 체력을 리셋
    void FindAndAssignUI()
    {
        // 1. 씬에서 "GameUI" 태그를 가진 Canvas를 찾음
        GameObject uiRoot = GameObject.FindWithTag("GameUI");
        if (uiRoot == null)
        {
            Debug.LogError("GameManager가 'GameUI' 태그를 가진 Canvas를 찾지 못했습니다! Core 씬의 Canvas에 'GameUI' 태그를 설정했는지 확인하세요!");
            return;
        }

        // 최상위 캔버스 할당
        gameUIRoot = uiRoot;

        // 2. UI 참조를 '안전하게' 다시 찾습니다.
        Transform tempT; // 임시 Transform 변수

        // 하트 UI 초기화
        UIhealth = new Image[3]; 

        // Health1
        tempT = uiRoot.transform.Find("Health1");
        if (tempT == null) Debug.LogError("GameUI에서 'Health1' 오브젝트를 찾지 못했습니다!");
        else UIhealth[0] = tempT.GetComponent<Image>();

        // Health2
        tempT = uiRoot.transform.Find("Health2");
        if (tempT == null) Debug.LogError("GameUI에서 'Health2' 오브젝트를 찾지 못했습니다!");
        else UIhealth[1] = tempT.GetComponent<Image>();

        // Health3
        tempT = uiRoot.transform.Find("Health3");
        if (tempT == null) Debug.LogError("GameUI에서 'Health3' 오브젝트를 찾지 못했습니다!");
        else UIhealth[2] = tempT.GetComponent<Image>();

        // (주의: "StageText"는 캔버스 자식 오브젝트의 실제 이름이어야 합니다.)
        tempT = uiRoot.transform.Find("Stage"); 
        if (tempT == null) Debug.LogError("GameUI에서 'Stage' 오브젝트를 찾지 못했습니다!");
        else UIStage = tempT.GetComponent<TMP_Text>();

        // MissionText
        tempT = uiRoot.transform.Find("MissionText");
        if (tempT == null) Debug.LogError("GameUI에서 'MissionText' 오브젝트를 찾지 못했습니다!");
        else UIMissionText = tempT.GetComponent<TMP_Text>();

        // ItemGetPopup
        tempT = uiRoot.transform.Find("ItemGetPopup");
        if (tempT == null) Debug.LogError("GameUI에서 'ItemGetPopup' 오브젝트를 찾지 못했습니다!");
        else
        {
            itemPopupPanel = tempT.gameObject;
            Transform popupTextT = itemPopupPanel.transform.Find("PopupText");
            if (popupTextT == null) Debug.LogError("ItemGetPopup에서 'PopupText' 오브젝트를 찾지 못했습니다!");
            else itemPopupText = popupTextT.GetComponent<TMP_Text>();
        }

        // MachinePopup
        tempT = uiRoot.transform.Find("MachinePopup");
        if (tempT == null) Debug.LogError("GameUI에서 'MachinePopup' 오브젝트를 찾지 못했습니다!");
        else machineCompletionPanel = tempT.gameObject;

        // InactivityPopup
        tempT = uiRoot.transform.Find("InactivityPopup");
        if (tempT == null) Debug.LogError("GameUI에서 'InactivityPopup' 오브젝트를 찾지 못했습니다!");
        else inactivityPopup = tempT.gameObject;

        // [ 3. 체력/UI 리셋 로직 ]
        health = 3;
        if (UIhealth != null && UIhealth.Length > 0 && UIhealth[0] != null)
        {
            for (int i = 0; i < UIhealth.Length; i++)
            {
                if (UIhealth[i] != null && fullHeartSprite != null)
                {
                    UIhealth[i].sprite = fullHeartSprite;
                }
            }
        }
        else
        {
            Debug.LogError("UIhealth 배열을 찾거나 채우는 데 실패했습니다!");
        }
        Debug.LogWarning("GameManager가 UI 참조를 모두 다시 연결하고 체력을 리셋했습니다.");
    }

    public void RegisterPlayer(PlayerMove newPlayer)
    {
        Debug.LogWarning("새로운 Player가 GameManager에 등록되었습니다!");
        
        // 1. 'player' 변수를 새로 등록된 플레이어로 설정
        player = newPlayer;
        // [ ★★★ 수정 ★★★ ] Rigidbody도 새로고침
        if (player != null)
            playerRigidbody = player.GetComponent<Rigidbody2D>();

        // 2. 카메라 참조도 새로 찾음
        if (mainCamera == null)
        {
            GameObject cameraObj = GameObject.FindWithTag("MainCamera");
            if (cameraObj != null)
                mainCamera = cameraObj.GetComponent<Camera>();
        }

        // 3. 부활 로직 (체력 리셋은 FindAndAssignUI가 담당)
        isDead = false; 
        player.enabled = true;
        player.Respawn(); // 물리/애니메이션 리셋

        // 4. 카메라를 새 플레이어의 자식으로 붙임
        if (mainCamera != null && player != null)
        {
            mainCamera.transform.SetParent(player.transform, true);
            mainCamera.transform.localPosition = new Vector3(0, 0, -10); // 원래 카메라 오프셋
        }

        // 5. 카메라 클램프가 있다면 타겟도 재설정
        if (mainCamera != null)
        {
            CameraClamp clampScript = mainCamera.GetComponent<CameraClamp>();
            if (clampScript != null)
            {
                clampScript.target = player.transform;
            }
        }
    }
    
    IEnumerator LoadInitialStage()
    {
        FreezePlayer(true); // 0. 씬 로드 동안 플레이어를 "얼림"

        // 1. stageIndex에 맞는 씬 이름을 가져옴
        string sceneToLoad = stageSceneNames[stageIndex];

        // 2. 해당 씬을 추가로 로드
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
        {
            yield return null; // 로드 완료까지 대기
        }

        // [ ★★★ 수정 ★★★ ]
        // 씬 로드 완료! Stage.cs의 Start()가 실행될 시간을 1프레임 줍니다.
        yield return null; 

        // 3. 이제 Stage.cs가 등록되었는지 확인
        float waitTimer = 0f;
        while (currentStage == null)
        {
            if (waitTimer > 3.0f) 
            {
                if (stageSceneNames != null && stageIndex < stageSceneNames.Count)
                {
                    Debug.LogError(stageSceneNames[stageIndex] + " 씬에 Stage.cs가 없거나 등록에 실패했습니다!");
                }
                else
                {
                    Debug.LogError("Stage 등록 실패 (stageSceneNames가 null이거나 index 오류)");
                }
                FreezePlayer(false); 
                yield break; // 코루틴 중단
            }
            waitTimer += Time.deltaTime;
            yield return null;
        }

        // 새 씬의 카메라 경계를 찾으라고 카메라에게 알림
        if (mainCamera != null)
        {
            mainCamera.GetComponent<CameraClamp>()?.FindNewBoundary();
        }

        // 4. 플레이어를 스폰 지점으로 이동
        Debug.Log(sceneToLoad + " 로드 완료 및 Stage 등록 완료.");
        PlayerReposition();

        // 스테이지에 맞는 미션 설정
        SetupMissionForStage(stageIndex);

        // 5. 모든 로드/재배치가 끝났으므로 플레이어를 활성화
        FreezePlayer(false);
    }

    void Update()
    {
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
        if (isChangingStage) return;

        // 1. Stage 4 (인덱스 3)이고, *아직* Memory 씬을 완료하지 않았다면
        if (stageIndex == 3 && GameData.HasCompletedMemory == false) 
        {
            if (cardKeysCollected < cardKeysNeeded)
            {
                StartCoroutine(ShowItemPopup(stage4LockedMessage));
                return; 
            }
        }
        // 3. Stage 4 (인덱스 3)이고, *이미* Memory 씬을 완료했다면
        else if (stageIndex == 3 && GameData.HasCompletedMemory == true)
        {
            // 4. (성공) Stage 5 (새 인덱스 5)으로 "점프"
            Debug.Log("Stage4 -> Stage5 (인덱스 5)으로 점프합니다.");
            
            // 'jumpToStage5' 플래그를 true로 넘깁니다.
            StartCoroutine(ChangeStageRoutine(true)); // (true = 점프)
            return; 
        }
        
        // 6. 그 외의 경우 (Stage1->2, Stage2->3, Stage4->Memory 등)
        if (stageIndex < stageSceneNames.Count - 1)
        {
            StartCoroutine(ChangeStageRoutine(false)); // (false = 점프 아님)
        }
    }

    // 씬을 비동기(Async)로 로드/언로드하는 코루틴
    IEnumerator ChangeStageRoutine(bool jumpToStage5) 
    {
        isChangingStage = true;
        FreezePlayer(true); 

        // 3. 현재 스테이지 씬을 언로드
        AsyncOperation asyncUnload = null;
        if (stageIndex >= 0 && stageIndex < stageSceneNames.Count && !string.IsNullOrEmpty(stageSceneNames[stageIndex]))
        {
            // [ ★ 수정 ★ ] (1번 맵 겹침 문제 해결)
            // 이름(string) 대신 로드된 씬(Scene) 객체로 언로드
            Scene currentScene = SceneManager.GetSceneByName(stageSceneNames[stageIndex]);
            if (currentScene.IsValid())
            {
                asyncUnload = SceneManager.UnloadSceneAsync(currentScene);
                Debug.Log(currentScene.name + " 씬을 언로드합니다.");
            }
            else
            {
                Debug.LogWarning(stageSceneNames[stageIndex] + " 씬을 찾지 못해 언로드에 실패했습니다. (이름 확인 필요)");
            }
            currentStage = null; 
        }

        // 4. 씬 언로드가 끝날 때까지 기다림
        while (asyncUnload != null && !asyncUnload.isDone)
        {
            yield return null;
        }

        // 5. 인덱스 계산 (3번 씬 언로드 문제 해결)
        if (jumpToStage5)
        {
            stageIndex = 5; // "Stage_5" 씬의 인덱스(5)로 강제 점프
        }
        else
        {
            stageIndex++; // 일반적인 다음 스테이지
        }

        // 6. 다음 씬 로드
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(stageSceneNames[stageIndex], LoadSceneMode.Additive);
        while (!asyncLoad.isDone) { yield return null; }

        yield return null; // 7. Stage.cs 대기

        // 8. Stage.cs 등록 확인 (3번 무한 추락 문제 해결)
        float waitTimer = 0f;
        while (currentStage == null)
        {
            if (waitTimer > 3.0f) 
            {
                Debug.LogError(stageSceneNames[stageIndex] + " 씬에 Stage.cs가 없거나 등록에 실패했습니다! (2단계 확인 필요)");
                FreezePlayer(false); 
                yield break; 
            }
            waitTimer += Time.deltaTime;
            yield return null;
        }

        // 9. UI 및 미션 갱신
        if (UIStage != null) UIStage.text = "STAGE " + (stageIndex + 1);
        SetupMissionForStage(stageIndex); // (2번 BGM 문제 해결)
        if (mainCamera != null) mainCamera.GetComponent<CameraClamp>()?.FindNewBoundary();
  
        // 10. 플레이어 재배치 및 활성화
        PlayerReposition();
        FreezePlayer(false);

        // [수정] 11. (1번 맵 겹침 문제 해결) 중복 호출 방지
        yield return new WaitForSeconds(0.1f);
        isChangingStage = false;
    }

    public void HealthDown()
    {
        if (isDead || player == null) return;

        if (health > 1)
        {
            health--;
            UIhealth[health].sprite = emptyHeartSprite;
        }
        else
        {
            // "죽음" 상태로 변경
            isDead = true;
            health = 0;

            if (UIhealth != null && UIhealth.Length > 0 && UIhealth[0] != null)
                UIhealth[0].sprite = emptyHeartSprite;

            // Player Die Effect
            if (player != null)
            {
                player.OnDie(); // 1. "Die" 애니메이션 재생 명령

                // 2. PlayerMove.cs 스크립트를 즉시 비활성화 (Update 덮어쓰기 방지)
                player.enabled = false; 
            }
            else
            {
                Debug.LogError("HealthDown: player 참조가 null입니다!");
                return;
            }

            Debug.Log("죽었습니다.");

            // "GameData.cs"에 현재 스테이지 인덱스 저장
            GameData.StageToReload = stageIndex;
            StartCoroutine(PlayerDeathSequence());
        }
    }

    /// PreviousLife 씬의 '다시 시도' 버튼이 호출할 함수
    public void ReloadStageAfterDeath()
    {
        // 1. PreviousLife 씬을 언로드
        SceneManager.UnloadSceneAsync(cutsceneSceneName);

        // 2. UI 참조를 다시 찾고 UI를 켬
        FindAndAssignUI(); 
        if (gameUIRoot != null)
            gameUIRoot.SetActive(true); // 1번(캔버스 겹침) 문제 해결

        // [수정] 2번(설정 리셋) 문제 해결
        if (mainCamera != null)
            mainCamera.orthographicSize = defaultCameraSize; // 카메라 줌 원상복구

        // 3. 플레이어 상태 리셋
        isDead = false;
        Time.timeScale = 1;
        
        // 4. 플레이어 스크립트에 "부활" 신호 보내기 (물리 상태 리셋)
        if (player != null)
        {
            player.Respawn(); 
        }
        else
        {
            Debug.LogError("플레이어 참조가 없습니다! Core 씬을 확인하세요.");
            return;
        }

        // 5. GameData에서 죽었던 스테이지 인덱스 가져오기
        if (GameData.StageToReload >= 0)
        {
            stageIndex = GameData.StageToReload;
            GameData.StageToReload = -1; 
        }

        // 6. 스테이지 UI 텍스트 업데이트
        if (UIStage != null)
        {
            UIStage.text = "STAGE " + (stageIndex + 1);
        }
        
        // 7. 스테이지 로드 코루틴 실행
        // (LoadInitialStage가 SetupMissionForStage를 호출하여 BGM을 다시 켬 - 2번 문제 해결)
        StartCoroutine(LoadInitialStage());
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
            // 혹시 모르니 예외 처리
            if (currentStage == null)
            {
                Debug.LogWarning("PlayerReposition: currentStage가 null입니다. Stage.cs가 등록되지 않았습니다.");
            }
            else if (currentStage.spawnPoint == null)
            {
                // 이게 스폰 실패의 원인일 수 있습니다.
                Debug.LogError("PlayerReposition: " + currentStage.name + "의 spawnPoint가 null입니다! 씬에서 스폰 지점을 연결했는지 확인하세요.");
            }
            player.transform.position = new Vector3(0, 0, -1);
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
        // 1. BGM 중지 (GameManager는 살아남으므로 필요함)
        StopMusic(); 

        // 2. Die 애니메이션이 재생될 시간을 잠시 기다림
        yield return new WaitForSeconds(0.5f); 

        // 3. 줌인 연출 (씬이 넘어가기 전까지 보여줌)
        Vector3 targetPosition = new Vector3(
            player.headTransform.position.x,
            player.headTransform.position.y,
            mainCamera.transform.position.z
        );
        Vector3 startPosition = mainCamera.transform.position;
        float startZoomSize = mainCamera.orthographicSize;

        float timer = 0f;

        // 4. 줌인 루프
        while (timer < deathZoomDuration)
        {
            float t = timer / deathZoomDuration;
            float smoothT = t * t * (3f - 2f * t); 

            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, smoothT);
            mainCamera.orthographicSize = Mathf.Lerp(startZoomSize, targetZoomSize, smoothT);

            timer += Time.deltaTime;
            yield return null; 
        }

        // 5. 줌인 완료 후 잠시 대기
        yield return new WaitForSeconds(0.8f);

        // 6. "PreviousLife" 씬을 'Single' 모드로 로드
        // (GameData.StageToReload는 HealthDown에서 이미 저장됨)
        // 이 코드는 DontDestroyOnLoad(GameManager)를 제외한
        // Core 씬의 모든 것(Player, UI, Camera)을 파괴합니다.
        SceneManager.LoadScene(cutsceneSceneName);
    }

    // 자리 비움 팝업
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
        SceneManager.LoadScene("#00MainMenu");
    }

    // 스테이지별 미션
    void SetupMissionForStage(int index)
    {
        AudioClip clipToPlay = null;

        // Stage 1 (index 0) 또는 Stage 2 (index 1)
        if (index == 0 || index == 1)
        {
            clipToPlay = musicStage1_2;
            UIMissionText.text = ""; 
        }
        // Stage 3 (index 2)
        else if (index == 2)
        {
            clipToPlay = musicStage3;
            UIMissionText.text = ""; 
        }
        // Stage 4 (index 3)
        else if (index == 3)
        {
            clipToPlay = musicStage4;
            
            // (기존 미션 로직...)
            if (GameData.HasCompletedMemory) { UIMissionText.text = stage4Mission_Exit; }
            else if (cardKeysCollected >= cardKeysNeeded) { UIMissionText.text = stage4Mission_Complete; }
            else { UIMissionText.text = stage4Mission_InProgress; }
        }
        // Memory (index 4)
        else if (index == 4)
        {
            clipToPlay = musicMemory; 
            UIMissionText.text = ""; 
        }
        // Stage 5 (index 5)
        else if (index == 5)
        {
            clipToPlay = musicStage5;
            UIMissionText.text = ""; 
        }
        // Ending (index 6)
        else if (index == 6)
        {
            clipToPlay = musicEnding; // (musicEnding 대신 musicMemory 할당)
            UIMissionText.text = ""; 
        }
        // Credit (index 7)
        else if (index == 7)
        {
            clipToPlay = musicCredit;
            UIMissionText.text = "";
        }
        else
        {
            UIMissionText.text = "";
        }

        if (player != null) // (GameManager가 Player를 참조하고 있는지 확인)
        {
            // Index 3 = Stage 4 (새 씬 리스트 순서 기준)
            if (index == 3) 
            {
                // 스테이지 4 소리가 할당되었다면
                if (stage4WalkSound != null)
                    player.audioWalk = stage4WalkSound;
            }
            else // 그 외 모든 스테이지
            {
                // 기본 걷는 소리가 할당되었다면
                if (defaultWalkSound != null)
                    player.audioWalk = defaultWalkSound;
            }
        }

        // [음악 재생 로직]
        if (clipToPlay != null && (bgmPlayer.clip != clipToPlay || !bgmPlayer.isPlaying))
        {
            bgmPlayer.clip = clipToPlay; // 음악 교체
            bgmPlayer.Play(); // 재생
        }
    }

    // ItemManager.cs가 이 함수를 호출
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
    }

    // 아이템 팝업을 2초간 띄웠다가 숨기는 코루틴
    IEnumerator ShowItemPopup(string message)
    {
        itemPopupText.text = message;
        itemPopupPanel.SetActive(true);

        yield return new WaitForSeconds(2.0f); // 2초 대기

        itemPopupPanel.SetActive(false);
    }

    // 미션이 완료되었는지 확인하는 함수
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
                // 아직 덜 모았다면
                UIMissionText.text = "카드키 (" + cardKeysCollected + "/" + cardKeysNeeded + ") 획득.";
            }
        }
    }

    // Machine.cs가 머신 상호작용 가능 여부를 물어볼 때 사용
    public bool IsCardKeyMissionComplete()
    {
        // 현재 스테이지가 Stage 4(index 3)이고, 키를 다 모았는지 확인
        if (stageIndex == 3 && cardKeysCollected >= cardKeysNeeded)
        {
            return true;
        }
        
        // 그 외의 경우는 모두 false
        return false;
    }

    // Machine.cs가 최종 상호작용을 요청할 때 호출
    public void ActivateCompletionPanel()
    {
        if (machineCompletionPanel != null)
        {
            machineCompletionPanel.SetActive(true);
            Debug.Log("미션 완료! 머신 패널이 활성화되었습니다.");
        }
        else
        {
            Debug.LogError("Machine Completion Panel이 GameManager에 연결되어 있지 않습니다!");
        }
    }

    public void StopMusic()
    {
        if (bgmPlayer != null)
        {
            bgmPlayer.Stop();
        }
    }

    // 로그인 패널(machineCompletionPanel)에서 "로그인 성공" 버튼이 호출할 함수
    public void OnCompletionPanelClosedSuccessfully()
    {
        StopMusic();

        // 1. "Memory" 씬을 봤다고 GameData에 플래그 설정
        GameData.HasCompletedMemory = true;

        // 2. 현재 스테이지(Stage 4)를 "돌아올 곳"으로 GameData에 저장
        //    이때 stageIndex는 3 (Stage 4)
        GameData.StageToReload = stageIndex;

        Debug.LogWarning("MEMORY씬 로드 직전: GameData.StageToReload = " + GameData.StageToReload);
    }

    // CreditTrigger가 호출할 크레딧 연출 시작 함수
    public void StartCreditPan(Transform target)
    {
        StartCoroutine(PanToCreditsRoutine(target));
    }

    IEnumerator PanToCreditsRoutine(Transform target)
    {
        // 1. 플레이어 조작을 막음
        isDead = true;
        player.enabled = false; // PlayerMove.cs 스크립트 자체를 비활성화

        // 2. 카메라가 플레이어를 따라다니는 것을 멈춤
        // (true를 전달하여 월드 좌표를 유지한 채 부모-자식 관계만 해제)
        mainCamera.transform.SetParent(null, true);

        // 3. 카메라를 'CreditCameraTarget'으로 패닝(이동)
        float timer = 0f;
        Vector3 startCamPos = mainCamera.transform.position;
        Vector3 targetCamPos = new Vector3(
            target.position.x,
            target.position.y,
            startCamPos.z // Z축은 그대로 유지
        );

        while (timer < creditPanDuration)
        {
            float t = timer / creditPanDuration;
            mainCamera.transform.position = Vector3.Lerp(startCamPos, targetCamPos, t * t);
            timer += Time.deltaTime;
            yield return null;
        }

        // 4. 패닝 완료 후 잠시 대기
        yield return new WaitForSeconds(1.0f);

        // 5. Stage 5 씬을 언로드 (제거)
        SceneManager.UnloadSceneAsync(stageSceneNames[stageIndex]); // stageIndex는 4 (Stage 5)
        
        // 6. "Credit" 씬을 Additive(추가) 모드로 로드
        //    (Core 씬과 카메라가 파괴되지 않음)
        SceneManager.LoadSceneAsync("Credit", LoadSceneMode.Additive);
    }

    // LoadStageAdditive 코루틴
    IEnumerator LoadStageAdditive(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        PlayerReposition(); // 씬 로드 후 플레이어 재배치
    }

    // 설정창 메인화면 돌아가기 함수
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        
        if (GameManager.Instance != null)
        {
            // DontDestroyOnLoad로 등록된 객체를 파괴합니다.
            Destroy(GameManager.Instance.gameObject); 
            GameManager.Instance = null; // 인스턴스 참조도 확실히 제거
        }
        
        // "MainMenu" 씬을 로드합니다.
        SceneManager.LoadScene("#00MainMenu");
    }
    
    // 게임 종료 버튼
    public void QuitGame()
    {
        Application.Quit();
    }

    public void ReturnToStage4FromEnding()
    {
        Debug.Log("Ending 씬에서 Stage4로 돌아갑니다.");

        // 1. Ending 씬을 언로드
        // (stageIndex가 Ending(5)을 가리키고 있어야 함)
        if (stageIndex >= 0 && stageIndex < stageSceneNames.Count && !string.IsNullOrEmpty(stageSceneNames[stageIndex]))
        {
            // 현재 씬(Ending) 언로드
            SceneManager.UnloadSceneAsync(stageSceneNames[stageIndex]);
        }
        else
        {
            SceneManager.UnloadSceneAsync("Ending"); // 비상시 이름으로 직접 언로드
        }

        // 2. UI 참조를 다시 찾고 UI를 켬
        FindAndAssignUI(); 
        if (gameUIRoot != null)
            gameUIRoot.SetActive(true);

        // 3. 플레이어 상태 리셋
        isDead = false;
        Time.timeScale = 1;
        
        // 4. 플레이어 스크립트 활성화
        if (player != null)
        {
            player.Respawn();
        }

        // [수정] 3번(Ending) 문제 해결
        // 5. Stage4 (인덱스 3)로 돌아가도록 stageIndex를 강제 지정
        // (Memory 씬 진입 전 GameData.StageToReload에 3이 저장되었어야 함)
        if (GameData.StageToReload == 3) // Stage 4에서 왔는지 확인
        {
            stageIndex = GameData.StageToReload; 
            GameData.StageToReload = -1; // 사용했으니 리셋
        }
        else
        {
            stageIndex = 3; // 비상시 Stage 4(인덱스 3)로 강제 설정
        }

        // 6. 스테이지 UI 텍스트 업데이트
        if (UIStage != null)
        {
            UIStage.text = "STAGE " + (stageIndex + 1);
        }
        
        // 7. Stage4 씬 로드 코루틴 실행
        StartCoroutine(LoadInitialStage());
    }

    // 씬이 로드되었을 때 SceneManager가 호출하는 함수
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 로드된 씬이 "Core" 씬이라면 (즉, 우리가 부활해서 Core로 돌아왔다면)
        if (scene.name == "Core")
        {
            Debug.LogWarning("GameManager가 Core 씬 로드를 감지했습니다. 게임을 다시 초기화합니다.");
            // Start()에 있던 모든 초기화 로직을 다시 실행
            InitializeGame();
        }
    }

    // GameManager가 파괴될 때 호출됨 (메모리 누수 방지)
    void OnDestroy()
    {
        // 구독했던 씬 로드 이벤트를 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 플레이어를 물리적으로 얼리거나 해제함
    private void FreezePlayer(bool freeze)
    {
        if (player == null || playerRigidbody == null)
        {
            Debug.LogWarning("FreezePlayer: 플레이어 또는 Rigidbody 참조가 없습니다.");
            return;
        }

        if (freeze)
        {
            // [얼리기]
            // 물리 영향(중력 등)을 받지 않도록 Kinematic으로 설정
            playerRigidbody.isKinematic = true; 
            playerRigidbody.velocity = Vector2.zero; // 현재 속도 제거
            player.enabled = false; // 스크립트 비활성화
        }
        else
        {
            // [해제]
            player.enabled = true; // 스크립트 활성화
            // 다시 물리 영향을 받도록 Dynamic으로 (Kinematic 해제)
            playerRigidbody.isKinematic = false; 
            // (PlayerReposition 후 Respawn이 호출되어야 함)
            player.Respawn(); 
        }
    }

}