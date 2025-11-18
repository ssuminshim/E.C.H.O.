using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class TypingEffect : MonoBehaviour
{
    public float typingSpeed = 0.05f;
    public bool IsTyping { get; private set; }

    private TMP_Text textMesh;
    private Coroutine typingCoroutine;

    void Awake()
    {
        textMesh = GetComponent<TMP_Text>();
    }

    public void StartTyping(string sentence)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        textMesh.text = "";
        typingCoroutine = StartCoroutine(ShowText(sentence));
    }

    public void SkipTyping(string sentence)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        textMesh.text = sentence;
        IsTyping = false;
    }

    private IEnumerator ShowText(string sentence)
    {
        IsTyping = true;
        foreach (char letter in sentence)
        {
            textMesh.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        IsTyping = false;
    }
}