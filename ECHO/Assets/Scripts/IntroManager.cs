using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Intro : MonoBehaviour
{
    public Text ScriptText_dialogue;
    public Text ScriptText_instruction;
    public string[] dialogue; // 인스펙터 창에서 대화 내용 수정 가능
    public int dialogue_count = 0;

    void Start()
    {
        // 게임 시작 시 첫 번째 대화 표시
        if (dialogue.Length > 0)
        {
            ScriptText_dialogue.text = dialogue[0];
        }
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
                ScriptText_dialogue.text = dialogue[dialogue_count];
            }
            // 마지막 대화가 끝나면 씬 전환
            else
            {
                SceneManager.LoadScene("Core");
                dialogue_count = 0; // 필요 시 초기화
            }
        }
    }

    // 메인메뉴로 돌아가는 함수
    public void ReturnToMainMenu()
    {
        // "MainMenu" 씬을 로드합니다.
        SceneManager.LoadScene("MainMenu");
    }
}
