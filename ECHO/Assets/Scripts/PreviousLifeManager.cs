using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using BackEnd;

public class PreviousLifeManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI MessageFromPast;

    public GameObject pressSpaceText;

    private bool canRetry = false; // 스페이스바를 누를 수 있는 상태인가?

    void Start()
    {
        // 서버에서 로그인 요청
        ResponseToLogin("test01", "1234");

        pressSpaceText.SetActive(false);
        canRetry = false;
        StartCoroutine(DialogueSequence());
    }

    IEnumerator DialogueSequence()
    {
        Debug.Log("회상씬 대화가 재생 중입니다...");
        yield return new WaitForSeconds(3.0f); // n초간 대기
        Debug.Log("회상씬 대화가 끝났습니다.");

        // "재도전" 텍스트를 킴
        pressSpaceText.SetActive(true);
        canRetry = true;
    }

void Update()
    {
        // "재도전"이 활성화됐고, 스페이스바를 눌렀다면
        if (canRetry && Input.GetKeyDown(KeyCode.Space))
        {
            // [수정!] 스페이스바 중복 입력을 막기 위해
            canRetry = false; 

            // [수정!] 살아있는 GameManager의 부활 함수를 호출
            GameManager gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                gameManager.ReloadStageAfterDeath();
            }
            else
            {
                Debug.LogError("GameManager.Instance를 찾을 수 없습니다! Core 씬이 파괴되었는지 확인하세요.");
                // 비상시: 어쩔 수 없이 Core 씬 로드 (버그가 다시 발생하겠지만)
                SceneManager.LoadScene("Core");
            }
        }
    }

    private void ResponseToLogin(string ID, string PW)
    {
        // 서버에 로그인 요청
        Backend.BMember.CustomLogin(ID, PW, callback =>
        {
            // 로그인 성공
            if (callback.IsSuccess())
            {
                // 서버에서 이전 플레이어의 메시지 불러오기
                GetMessage();
            }
            // 로그인 실패
            else
            {  
                string message = string.Empty;

                switch (int.Parse(callback.GetStatusCode()))
                {
                    case 401: // 존재하지 않는 아이디, 잘못된 비밀번호
                        message = callback.GetMessage().Contains("customID") ? "존재하지 않는 아이디입니다." : "잘못된 비밀번호";
                        break;
                    case 403: // 유저 or 디바이스 차단
                        message = callback.GetMessage().Contains("user") ? "차단당한 유저" : "차단당한 디바이스";
                        break;
                    case 410: // 탈퇴 진행중
                        message = "탈퇴 진행중";
                        break;
                    default:
                        message = callback.GetMessage();
                        break;
                }
                MessageFromPast.text = message;
            }

        });

    }

    private void GetMessage()
    {
        // 서버에서 이전 플레이어의 메시지 불러오기
        Backend.URank.User.GetRankList(Constants.DAILY_RANK_UUID, 1, callback =>
        {
            if (callback.IsSuccess())
            {
                // JSON 데이터 파싱 성공
                try
                {
                    Debug.Log($"랭킹 조회에 성공했습니다. : {callback}");

                    LitJson.JsonData rankDataJson = callback.FlattenRows();

                    // 받아온 데이터의 개수가 0이면 데이터가 없는 것
                    if (rankDataJson.Count <= 0)
                    {
                        Debug.LogWarning("데이터가 존재하지 않습니다.");
                    }
                    else
                    {
                        int rankerCount = rankDataJson.Count;
                        // 랭킹 정보를 불러와 출력할 수 있도록 설정
                        string message = rankDataJson[0]["Message"].ToString();
                        MessageFromPast.text = $"\"{message}\"";
                    }
                }
                // JSON 데이터 파싱 실패
                catch (System.Exception e)
                {
                    // try-catch 에러 출력
                    Debug.LogError(e);
                }
            }
            else
            {
                // 에러 텍스트 표시
                MessageFromPast.text = "Error: Rank";

                Debug.LogError($"랭킹 조회 중 오류가 발생했습니다. : {callback}");
            }
        });
    }
}

