using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections; // 코루틴을 위해 추가
using System; // Array.IndexOf 사용을 위해 추가

public class Intro : MonoBehaviour
{
    // --- Image 변수를 CanvasGroup으로 변경 (인스펙터에서 연결 필요) ---
    [SerializeField]
    private CanvasGroup imageGroup1;
    [SerializeField]
    private CanvasGroup imageGroup2;
    [SerializeField]
    private CanvasGroup imageGroup3;
    [SerializeField]
    private CanvasGroup imageGroup4;

    [Header("Fade Settings")]
    [Tooltip("이미지 페이드 인/아웃에 걸리는 시간 (초)")]
    public float imageFadeDuration = 0.5f; 
    
    public TMP_Text ScriptText_dialogue;
    public Text ScriptText_instruction;
    public TypingEffect dialogueTypingEffect;
    public string[] dialogue; // 인스펙터 창에서 대화 내용 수정 가능
    
    private CanvasGroup[] allImageGroups; // 모든 이미지 그룹을 관리할 배열

    private int dialogue_count = 0;
    private int sentenceIndex = 0;

    void Start()
    {
        // 모든 이미지 그룹 배열 초기화
        allImageGroups = new CanvasGroup[] { imageGroup1, imageGroup2, imageGroup3, imageGroup4 };

        // 1. 모든 이미지 그룹 초기 상태 설정 (투명하고 비활성화)
        foreach (var group in allImageGroups)
        {
            if (group != null)
            {
                group.alpha = 0f;
                group.gameObject.SetActive(false);
            }
        }

        // 2. 게임 시작 시 dialogue_count=0에 맞춰 이미지 활성화 상태 업데이트 (Fade In 시작)
        UpdateImageActivation();

        // 3. 첫 대화 시작
        ProceedToHeadlines();
    }

    void Update()
    {
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
        if (group == null || group.alpha >= 1f) yield break; // 이미 완전히 보이면 종료

        group.gameObject.SetActive(true); // GameObject 활성화
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
         if (dialogue.Length > 0)
        {
            sentenceIndex = 0;
            dialogueTypingEffect.StartTyping(dialogue[sentenceIndex]);
        }
        else
        {
            Debug.LogError("대화 텍스트가 비어있습니다. Inspector에서 입력해주세요.");
        }
    }

    void HandleSpacebarPress()
    {
            if (dialogueTypingEffect.IsTyping)
            {
                dialogueTypingEffect.SkipTyping(dialogue[sentenceIndex]);
            
            }
            else
            {
                sentenceIndex++;
                
                if (sentenceIndex < dialogue.Length)
                {
                    dialogue_count = sentenceIndex;
                    
                    // 🌟 dialogue_count가 증가할 때마다 이미지 활성화 상태 업데이트 및 Fade In 시작
                    UpdateImageActivation();
                    
                    dialogueTypingEffect.StartTyping(dialogue[sentenceIndex]);
                }
                else
                {
                    SceneManager.LoadScene("#02Loading");
                }
            }
    }
    

    /// <summary>
    /// dialogue_count 값에 따라 이미지들의 활성화 상태를 업데이트하고 Fade In을 시작합니다.
    /// </summary>
    private void UpdateImageActivation()
    {
        // 활성화해야 할 그룹 목록
        CanvasGroup[] groupsToActivate = new CanvasGroup[0];
        
        switch (dialogue_count)
        {
            case 0:
                groupsToActivate = new CanvasGroup[] { imageGroup1 };
                break;
            case 1:
                groupsToActivate = new CanvasGroup[] { imageGroup1, imageGroup2 };
                break;
            case 2:
                groupsToActivate = new CanvasGroup[] { imageGroup3 };
                break;
            case 3:
            case 4:
                groupsToActivate = new CanvasGroup[] { imageGroup3, imageGroup4 };
                break;
            default:
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

    // 메인메뉴로 돌아가는 함수
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("#00MainMenu");
    }
}