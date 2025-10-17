using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Memory : MonoBehaviour
{
    public GameObject player;
    public GameObject friend;

    public Text ScriptText_dialogue;
    public Text ScriptText_instruction;
    public Text ScriptText_name;
    public string[] dialogue; // 인스펙터 창에서 대화 내용 수정 가능
    public int dialogue_count = 0;

    void Start()
    {
        // 게임 시작 시 첫 번째 대화 표시
        if (dialogue.Length > 0)
        {
            ScriptText_dialogue.text = dialogue[0];
        }

        // 게임 시작 시 기본 이미지 활성화
        player.SetActive(true);
        friend.SetActive(true);

        // 게임 시작 시 첫 번째 이름 표시
        ScriptText_name.text = "???";

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
                SceneManager.LoadScene("Main_Game_Scene");
                dialogue_count = 0; // 필요 시 초기화
            }

            if (dialogue_count == 8)
            {
                ScriptText_name.text = "나";
            }
            else
            {
                ScriptText_name.text = "???";
            }
            
            // 이미지를 바꿀 대화 번호 설정
            // if (dialogue_count == 0 || dialogue_count == 3)
            // {
            //     player.SetActive(true);
            //     // player2.SetActive(false);
            //     ScriptText_name.text = "나";
            // }

            // // friend 기본 이미지 활성화
            // if (dialogue_count == 0 || dialogue_count == 3)
            // {
            //     friend.SetActive(true);
            //     // friend2.SetActive(false);
            //     ScriptText_name.text = "???";
            // }
        }
    }
}
