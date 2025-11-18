using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class CreditFlowManager : MonoBehaviour
{
    public GameObject HeadlinePanel;
    public GameObject headlineContent;
    public TypingEffect headlineTypingEffect;

    [Header("Headline Text")]
    [TextArea(3, 5)]
    public string headlineSentence;

    void Start()
    {
        HeadlinePanel.SetActive(true);

        ProceedToHeadlines();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HandleSpacebarPress();
        }
    }

    void ProceedToHeadlines()
    {   
        if (!string.IsNullOrEmpty(headlineSentence))
        {
            headlineTypingEffect.StartTyping(headlineSentence);
        }
        else
        {
            Debug.LogError("헤드라인 텍스트가 비어있습니다. Inspector에서 입력해주세요.");
        }
    }

    void HandleSpacebarPress()
    {
        if (headlineTypingEffect.IsTyping)
        {
            headlineTypingEffect.SkipTyping(headlineSentence);
        }
           else
        {
            HeadlinePanel.SetActive(false);
        }
    }
}