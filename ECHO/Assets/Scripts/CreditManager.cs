using System; // 에러 해결을 위해 추가
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CreditManager : MonoBehaviour
{
    // --- 이미지/패널 변수: CanvasGroup으로 변경 ---

    [Header("Panel Canvas Groups")]
    [SerializeField]
    private CanvasGroup finishMessageGroup; // 1. 마무리 멘트
    [SerializeField]
    private CanvasGroup endingImageGroup;   // 2. 엔딩 (E.C.H.O.) 
    [SerializeField]
    private CanvasGroup creditGroup;        // 3. 크레딧 패널
    [SerializeField]
    private CanvasGroup thankGroup;         // 4. 감사 패널
    [SerializeField]
    private CanvasGroup LastGroup;          // 5. 마지막 패널 (버튼)
    
    [Header("Headline Text & Typing")]
    public TypingEffect headlineTypingEffect;
    [TextArea(3, 5)]
    public string headlineSentence;

    [Header("Fade Settings")]
    [Tooltip("페이드 인/아웃에 걸리는 시간 (초)")]
    public float fadeDuration = 1.0f; // 페이드 인/아웃에 걸리는 시간

    
    void Start()
    {
        // 1. 모든 UI를 시작할 때 비활성화하고 초기 상태를 설정합니다.
        SetAllUIActive(false);

        // 2. 순차적으로 UI를 활성화하는 코루틴을 시작합니다.
        StartCoroutine(CreditSequenceRoutine());
    }

    /// <summary>
    /// 모든 UI 요소를 일괄적으로 비활성화하고 CanvasGroup을 초기화합니다.
    /// </summary>
    private void SetAllUIActive(bool isActive)
    {
        Action<CanvasGroup> initGroup = (group) =>
        {
            if (group != null)
            {
                group.alpha = 0f;
                group.blocksRaycasts = false;
                group.gameObject.SetActive(isActive); // 시작 시에는 false
            }
        };

        initGroup(finishMessageGroup);
        initGroup(endingImageGroup);
        initGroup(creditGroup);
        initGroup(thankGroup);
        initGroup(LastGroup); 
    }

    /// <summary>
    /// 특정 CanvasGroup을 페이드 인 시키는 코루틴입니다. (Alpha 0 -> 1)
    /// </summary>
    IEnumerator FadeInPanel(CanvasGroup group, float duration)
    {
        if (group == null) yield break;

        group.alpha = 0f;
        group.blocksRaycasts = false; 
        group.gameObject.SetActive(true); // GameObject 활성화

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            group.alpha = Mathf.Lerp(0f, 1f, timer / duration); 
            yield return null;
        }

        group.alpha = 1f;
        group.blocksRaycasts = true; // 상호작용 활성화
    }

    /// <summary>
    /// 특정 CanvasGroup을 페이드 아웃 시키는 코루틴입니다. (Alpha 1 -> 0)
    /// </summary>
    IEnumerator FadeOutPanel(CanvasGroup group, float duration)
    {
        if (group == null) yield break;

        group.blocksRaycasts = false; // 상호작용 비활성화

        float timer = 0f;
        float startAlpha = group.alpha;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            // 현재 알파 값에서 0f으로 보간
            group.alpha = Mathf.Lerp(startAlpha, 0f, timer / duration);
            yield return null;
        }

        group.alpha = 0f;
        group.gameObject.SetActive(false); // 알파가 0이 된 후 GameObject 비활성화
    }


    /// <summary>
    /// 요청하신 순서와 시간 간격에 따라 패널을 활성화합니다.
    /// </summary>
    IEnumerator CreditSequenceRoutine()
    {
        float waitTime = 8f; // 기본 대기 시간

        // 1. '마무리 멘트' 활성화 (페이드 인 없음, 즉시 등장)
        if (finishMessageGroup != null)
        {
            // 🌟 즉시 활성화
            finishMessageGroup.alpha = 1f;
            finishMessageGroup.blocksRaycasts = true;
            finishMessageGroup.gameObject.SetActive(true);

            if (!string.IsNullOrEmpty(headlineSentence) && headlineTypingEffect != null)
            {
                headlineTypingEffect.StartTyping(headlineSentence);
            }
            else
            {
                Debug.LogError("헤드라인 텍스트 또는 타이핑 효과 컴포넌트가 누락되었습니다.");
            }
            
            yield return new WaitForSeconds(waitTime + 3f);
            
            // 🌟 페이드 아웃 적용
            yield return StartCoroutine(FadeOutPanel(finishMessageGroup, fadeDuration)); 
        }
        
        // 2. '엔딩 (E.C.H.O.)' 활성화 (페이드 인, 페이드 아웃 적용)
        if (endingImageGroup != null)
        {
            yield return StartCoroutine(FadeInPanel(endingImageGroup, fadeDuration));
            yield return new WaitForSeconds(waitTime);
            yield return StartCoroutine(FadeOutPanel(endingImageGroup, fadeDuration));
        }

        // 3. '크레딧' 패널 활성화 (페이드 인, 페이드 아웃 적용)
        if (creditGroup != null)
        {
            yield return StartCoroutine(FadeInPanel(creditGroup, fadeDuration));
            yield return new WaitForSeconds(waitTime);
            yield return StartCoroutine(FadeOutPanel(creditGroup, fadeDuration));
        }

        // 4. '감사' 패널 활성화 (페이드 인, 페이드 아웃 적용)
        if (thankGroup != null)
        {
            yield return StartCoroutine(FadeInPanel(thankGroup, fadeDuration));
            yield return new WaitForSeconds(waitTime);
            yield return StartCoroutine(FadeOutPanel(thankGroup, fadeDuration));
        }
        
        // 🌟 5. 'LastPanel' 활성화 (버튼들이 포함된 최종 패널, 페이드 인만 적용)
        if (LastGroup != null)
        {
            yield return StartCoroutine(FadeInPanel(LastGroup, fadeDuration));
        }
    }


    // --- 버튼에 연결할 공용 함수 (이전과 동일) ---

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("#00MainMenu");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OpenListPanel()
    {
        Debug.Log("목록 버튼 클릭: '지금까지 남겨진 마음들' 목록 화면으로 이동합니다.");
    }
}