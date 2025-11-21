using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using BackEnd;
using System.Collections; // 코루틴을 위해 추가
using System;             // Array를 위해 추가
using TMPro;              // Text 대신 TMP_Text를 사용할 수도 있지만, 기존 Text 타입을 유지합니다.

public class Memory : MonoBehaviour
{
    // --- Image 변수를 CanvasGroup으로 변경 (인스펙터에서 연결 필요) ---
    [SerializeField]
    private CanvasGroup imageGroup1;
    [SerializeField]
    private CanvasGroup imageGroup2;
    [SerializeField]
    private CanvasGroup imageGroup3;

    [Header("Fade Settings")]
    [Tooltip("이미지 페이드 인에 걸리는 시간 (초)")]
    public float imageFadeDuration = 0.5f;

    public TMP_Text ScriptText_dialogue;
    public Text ScriptText_instruction;
    public Text ScriptText_name;
    public TypingEffect dialogueTypingEffect;
    public string[] dialogue; // 인스펙터 창에서 대화 내용 수정 가능
    
    // dialogue_count 변수는 사용하지 않고 sentenceIndex로 통합하여 사용합니다.
    private int sentenceIndex = 0;

    private CanvasGroup[] allImageGroups; // 모든 이미지 그룹을 관리할 배열
    private string MessageFromPast;


    void Start()
    {
        // 모든 이미지 그룹 배열 초기화 및 초기 상태 설정
        allImageGroups = new CanvasGroup[] { imageGroup1, imageGroup2, imageGroup3 };
        
        foreach (var group in allImageGroups)
        {
            if (group != null)
            {
                group.alpha = 0f;
                group.gameObject.SetActive(false);
            }
        }

        // 서버에서 이전 플레이어의 메시지 불러오기 (비동기)
        GetMessage();
        
        // 게임 시작 시 첫 번째 이름 표시
        ScriptText_name.text = "<앵커의 목소리>";

        // 게임 시작 시 dialogue_count=0에 맞춰 이미지 활성화 상태 업데이트 및 첫 대화 시작
        UpdateImageActivation();
        ProceedToHeadlines();
    }

    void Update()
    {
        // 스페이스바가 눌렸을 때 대화 진행
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HandleSpacebarPress();
        }
    }

    /// <summary>
    /// CanvasGroup을 0에서 1로 페이드 인 시키는 코루틴입니다.
    /// </summary>
    IEnumerator FadeInImage(CanvasGroup group, float duration)
    {
        if (group == null || group.alpha >= 1f) yield break;

        group.gameObject.SetActive(true);
        float startAlpha = group.alpha;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            group.alpha = Mathf.Lerp(startAlpha, 1f, timer / duration);
            yield return null;
        }
        group.alpha = 1f;
    }

    void ProceedToHeadlines()
    {
         if (dialogue.Length > 0 && dialogueTypingEffect != null)
        {
            // 첫 번째 대화 타이핑 시작 (sentenceIndex = 0)
            dialogueTypingEffect.StartTyping(dialogue[sentenceIndex]);
        }
        else
        {
            Debug.LogError("대화 텍스트 또는 타이핑 효과 컴포넌트가 누락되었습니다.");
        }
    }

    void HandleSpacebarPress()
    {
        string currentSentence = dialogue[sentenceIndex];
        if (sentenceIndex == 14 && sentenceIndex < dialogue.Length)
        {
             currentSentence = $"그래서 난 \"{MessageFromPast}\"(이)라고 남길거야.";
        }
        
        if (dialogueTypingEffect.IsTyping)
        {
            // 타이핑 중이면 스킵: 특수 처리된 텍스트 (currentSentence)를 사용합니다.
            dialogueTypingEffect.SkipTyping(currentSentence); // 👈 **수정된 부분**
        }
        else
        {
            // 타이핑 완료 후 다음 대화로 진행
            sentenceIndex++;
            
            if (sentenceIndex < dialogue.Length)
            {
                // 특수 대화 처리 (인덱스 14)
                string nextSentence = dialogue[sentenceIndex];
                if (sentenceIndex == 14)
                {
                    nextSentence = $"그래서 난 \"{MessageFromPast}\"(이)라고 남길거야.";
                }

                // 특수 이름 변경 처리
                UpdateNameText(sentenceIndex);

                // 이미지 활성화 상태 업데이트 및 Fade In 시작
                UpdateImageActivation();
                
                // 다음 대화 타이핑 시작
                dialogueTypingEffect.StartTyping(nextSentence);
            }
            else
            {
                // 마지막 대화가 끝나면 씬 전환
                GameData.HasCompletedMemory = true;
                GameData.StageToReload = 2;
                SceneManager.LoadScene("Core");
            }
        }
    }

    /// <summary>
    /// sentenceIndex 값에 따라 이름 텍스트를 업데이트합니다.
    /// </summary>
    private void UpdateNameText(int index)
    {
        if (index == 0)
        {
            // 0일 때: <앵커의 목소리>
            ScriptText_name.text = "<앵커의 목소리>";
        }
        else if (index == 18)
        {
            // 18일 때: <나>
            ScriptText_name.text = "<나>";
        }
        else if (index == 19)
        {
            // 19일 때: <기계>
            ScriptText_name.text = "<기계>";
        }
        else
        {
            // 0, 18, 19를 제외한 모든 인덱스: <???>
            ScriptText_name.text = "<???>";
        }
    }


    /// <summary>
    /// sentenceIndex 값에 따라 이미지들의 활성화 상태를 업데이트하고 Fade In을 시작합니다.
    /// </summary>
    private void UpdateImageActivation()
    {
        CanvasGroup[] groupsToActivate = new CanvasGroup[0];
        
        switch (sentenceIndex)
        {
            case 0:
            case 19: // All off
                groupsToActivate = new CanvasGroup[] {}; 
                break;
            case 7:
            case 8:
            case 15:
            case 18:
                groupsToActivate = new CanvasGroup[] { imageGroup2 };
                break;
            case 16:
            case 17:
                groupsToActivate = new CanvasGroup[] { imageGroup3 };
                break;
            default: // case 1-6, 9-14, 20+ (이전 코드의 default: image1.gameObject.SetActive(true);에 해당)
                groupsToActivate = new CanvasGroup[] { imageGroup1 };
                break;
        }
        
        // 1. 비활성화해야 할 그룹 처리: 활성화 목록에 없으면 즉시 비활성화
        foreach (var group in allImageGroups)
        {
            if (group != null)
            {
                // Array.IndexOf로 현재 그룹이 활성화 목록에 있는지 확인
                if (Array.IndexOf(groupsToActivate, group) == -1)
                {
                    // 활성화 목록에 없으면 즉시 비활성화 및 알파 초기화
                    group.gameObject.SetActive(false);
                    group.alpha = 0f;
                }
            }
        }
        
        // 2. 활성화해야 할 그룹 처리: Fade In 코루틴 시작
        foreach (var group in groupsToActivate)
        {
            if (group != null)
            {
                // 이미 완전히 불투명한 상태가 아니라면 Fade In 시작
                if (group.alpha < 1f)
                {
                    StartCoroutine(FadeInImage(group, imageFadeDuration));
                }
            }
        }
    }

    private void GetMessage()
    {
        // 서버에서 이전 플레이어의 메시지 불러오기
        Backend.URank.User.GetRankList(Constants.DAILY_RANK_UUID, 1, callback =>
        {
            if (callback.IsSuccess())
            {
                try
                {
                    Debug.Log($"랭킹 조회에 성공했습니다. : {callback}");
                    LitJson.JsonData rankDataJson = callback.FlattenRows();
                    if (rankDataJson.Count > 0)
                    {
                        // 랭킹 정보를 불러와 MessageFromPast에 저장
                        MessageFromPast = rankDataJson[0]["Message"].ToString();
                    }
                    else
                    {
                        Debug.LogWarning("데이터가 존재하지 않아 기본 메시지를 사용합니다.");
                        MessageFromPast = "이전에 메시지를 남긴 사람이 없습니다."; 
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"JSON 파싱 실패: {e}");
                    MessageFromPast = "Error: JSON Parse";
                }
            }
            else
            {
                // 에러 발생 시 기본 텍스트 표시
                MessageFromPast = "Error: Rank Load";
                Debug.LogError($"랭킹 조회 중 오류가 발생했습니다. : {callback}");
            }
        });
    }
}