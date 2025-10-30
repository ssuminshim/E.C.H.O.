using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BackEnd;
using UnityEngine.SocialPlatforms.Impl;
using System;
using UnityEngine.SceneManagement;

public class TopPanelViewer : MessageBass
{
    [SerializeField]
    private Image               imageMessage;

    [SerializeField]
    private TMP_InputField      inputFieldMessage;

    [SerializeField]
    private Button              btnMessage;

    [SerializeField]
    private DailyRankRegister dailyRank;

    private void Awake()
    {
        BackendGameData.Instance.onGameDataLoadEvent.AddListener(UpdateGameData);
    }

    public void UpdateGameData()
    {
        inputFieldMessage.text = BackendGameData.Instance.UserGameData.Message;
    }


    public void OnClickMessage()
    {
        ResetUI(imageMessage);

        // 필드 값이 비어있는지 체크
        if (IsFieldDataEmpty(imageMessage, inputFieldMessage.text)) return;

        // 버튼 연타하지 못하도록 상호작용 비활성화
        btnMessage.interactable = false;

        // 1. 현재 날짜와 시간 가져오기
        DateTime now = DateTime.Now;

        // 2. "MMddHHmm"으로 문자열을 포맷
        string dateString = now.ToString("MMddHHmm");

        // 3. 포맷된 문자열을 정수(int)로 변환
        if (int.TryParse(dateString, out int dateInt))
        {
            // 변환 성공
            Debug.Log("현재 날짜/시간 정수 (MMddHHmm): " + dateInt);
            // dateInt 변수에 10291444와 같은 정수 값 저장
            // 예시: 10월 29일 오후 2시 44분 활성화 시
            // 출력: 현재 날짜/시간 정수 (MMddHHmm): 10291444
        }
        else
        {
            // 변환 실패
            Debug.LogError("날짜/시간을 정수로 변환하는 데 실패했습니다.");
        }

        // 현재 날짜 정보를 바탕으로 랭킹 데이터 갱신
        dailyRank.Process(dateInt);

        // 메시지 업데이트
        BackendGameData.Instance.UserGameData.Message = inputFieldMessage.text;

        // 게임 정보 업데이트
        BackendGameData.Instance.GameDataUpdate(AfterMessageUpdate);
    }

    public void AfterMessageUpdate()
    {
        Debug.Log("게임 정보 업데이트 완료.");

        SceneManager.LoadScene("Credit");
    }
}