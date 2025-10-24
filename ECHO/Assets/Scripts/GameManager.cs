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
    
    // [변경됨] GameObject 배열 대신 씬 이름 리스트로 변경
    public List<string> stageSceneNames = new List<string>();
    
    // [추가됨] 현재 스테이지의 정보를 저장할 변수
    private Stage currentStage; 

    public Image[] UIhealth;
    public TMP_Text UIPoint;
    public TMP_Text UIStage;
    public GameObject UIRestartBtn;

    void Start()
    {
        // 게임 시작 시 첫 스테이지 UI 텍스트 설정
        UIStage.text = "STAGE " + (stageIndex + 1);
    }

    void Update()
    {
        UIPoint.text = (totalPoint + stagePoint).ToString();
    }

    // [추가됨] Stage.cs가 호출할 메서드
    public void RegisterStage(Stage stage)
    {
        currentStage = stage;
        Debug.Log(stage.name + " 스테이지가 등록되었습니다.");
    }

    // [수정됨] NextStage 로직을 코루틴으로 분리
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
            // [버그 수정] Text -> TMP_Text
            TMP_Text btnText = UIRestartBtn.GetComponentInChildren<TMP_Text>();
            if (btnText != null)
                btnText.text = "Clear!";
            UIRestartBtn.SetActive(true);
        }
    }

    // [추가됨] 씬을 비동기(Async)로 로드/언로드하는 코루틴
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
        if (health > 1)
        {
            health--;
            UIhealth[health].color = new Color(1, 0, 0, 0.4f);
        }
        else
        {
            // All Health UI Off
            UIhealth[0].color = new Color(1, 0, 0, 0.4f);

            // Player Die Effect
            player.OnDie();

            // Result UI
            Debug.Log("죽었습니다.");

            // Retry Button UI
            UIRestartBtn.SetActive(true);
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

    // [수정됨] 현재 스테이지의 스폰 지점을 사용
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
        // 씬 0번(아마도 Core 씬 또는 모든 것을 로드하는 Loader 씬)을 로드
        SceneManager.LoadScene(0); 
    }
}