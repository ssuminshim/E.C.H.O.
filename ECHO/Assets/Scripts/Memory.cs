using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using BackEnd;

public class Memory : MonoBehaviour
{
    [SerializeField]
    private Image image1;
    [SerializeField]
    private Image image2;
    [SerializeField]
    private Image image3;

    public Text ScriptText_dialogue;
    public Text ScriptText_instruction;
    public Text ScriptText_name;
    public string[] dialogue; // 인스펙터 창에서 대화 내용 수정 가능
    public int dialogue_count = 0;

    private string MessageFromPast;

    void Start()
    {
        // 서버에서 이전 플레이어의 메시지 불러오기
        GetMessage();

        // 게임 시작 시 첫 번째 대화 표시
        if (dialogue.Length > 0)
        {
            ScriptText_dialogue.text = dialogue[0];
        }

        // 게임 시작 시 dialogue_count=0에 맞춰 이미지 초기화
        UpdateImageActivation();

        // 게임 시작 시 첫 번째 이름 표시
        ScriptText_name.text = "<앵커의 목소리>";

    }

    void Update()
    {
        // 스페이스바가 눌렸을 때 대화 진행
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 대화 인덱스가 배열 크기를 초과하는지 확인
            if (dialogue_count < dialogue.Length - 1)
            {
                dialogue_count++;

                if (dialogue_count == 14)
                {
                    ScriptText_dialogue.text = $"그래서 난 \"{MessageFromPast}\"라고 남길거야.";
                }
                else
                {
                    ScriptText_dialogue.text = dialogue[dialogue_count];
                }
                

                // dialogue_count가 증가할 때마다 이미지 활성화 상태 업데이트
                UpdateImageActivation();
            }
            // 마지막 대화가 끝나면 씬 전환
            else
            {
                GameData.HasCompletedMemory = true;
                GameData.StageToReload = 3;
                SceneManager.LoadScene("Core");
            }

            if (dialogue_count == 18)
            {
                ScriptText_name.text = "<나>";
            }
            if (dialogue_count == 19)
            {
                ScriptText_name.text = "<기계>";
            }
            if (dialogue_count > 19)
            {
                ScriptText_name.text = " ";
            }
            else
            {
                ScriptText_name.text = "<???>";
            }
        }
    }

    /// <summary>
    /// dialogue_count 값에 따라 이미지들의 활성화 상태를 업데이트
    /// </summary>
    private void UpdateImageActivation()
    {
        // 모든 이미지 비활성화
        image1.gameObject.SetActive(false);
        image2.gameObject.SetActive(false);
        image3.gameObject.SetActive(false);

        // dialogue_count 값에 따라 특정 이미지 활성화
        switch (dialogue_count)
        {
            case 0:
                image1.gameObject.SetActive(false);
                image2.gameObject.SetActive(false);
                image3.gameObject.SetActive(false);
                break;
            case 7:
            case 8:
                image2.gameObject.SetActive(true);
                break;
            case 15:
                image2.gameObject.SetActive(true);
                break;
            case 16:
            case 17:
                image3.gameObject.SetActive(true);
                break;
            case 18:
                image2.gameObject.SetActive(true);
                break;
            case 19:
                image1.gameObject.SetActive(false);
                image2.gameObject.SetActive(false);
                image3.gameObject.SetActive(false);
                break;
            default:
                image1.gameObject.SetActive(true);
                break;
        }
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
                        MessageFromPast = rankDataJson[0]["Message"].ToString();
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
                MessageFromPast = "Error: Rank";

                Debug.LogError($"랭킹 조회 중 오류가 발생했습니다. : {callback}");
            }
        });
    }
}

