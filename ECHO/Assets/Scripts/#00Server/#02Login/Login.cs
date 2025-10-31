using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BackEnd;
using UnityEngine.SceneManagement;

public class Login : LoginBass
{
    [SerializeField]
    private Image imageID;
    [SerializeField]
    private TMP_InputField inputFieldID;
    [SerializeField]
    private Image imagePW;
    [SerializeField]
    private TMP_InputField inputFieldPW;

    [SerializeField]
    private Button btnLogin;

    public void OnClickLogin()
    {
        ResetUI(imageID, imagePW);

        // 필드 값이 비어있는지 체크
        if (IsFieldDataEmpty(imageID, inputFieldID.text, "이름")) return;
        if (IsFieldDataEmpty(imagePW, inputFieldPW.text, "비밀번호")) return;

        // 로그인 버튼 연타하지 못하도록 상호작용 비활성화
        btnLogin.interactable = false;

        // 서버에 로그인을 요청하는 동안 화면에 출력하는 내용 업데이트
        // ex) 로그인 관련 텍스트 출력, 톱니바퀴 아이콘 회전 등
        StartCoroutine(nameof(LoginProcess));

        // 뒤끝 서버 로그인 시도
        ResponseToLogin(inputFieldID.text, inputFieldPW.text);

    }

    // 로그인 시도 후 서버로부터 전달받은 message를 기반으로 로직 처리

    private void ResponseToLogin(string ID, string PW)
    {
        // 서버에 로그인 요청
        Backend.BMember.CustomLogin(ID, PW, callback =>
        {
            StopCoroutine(nameof(LoginProcess));

            // 로그인 성공
            if (callback.IsSuccess())
            {
                SetMessage($"{inputFieldID.text}님 환영합니다.");

                // Memory 씬으로 이동
                SceneManager.LoadScene("Memory");

            }
            // 로그인 실패
            else
            {
                btnLogin.interactable = true;

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

                // StatusCode 401에서 "잘못된 비밀번호 입니다"일때
                if (message.Contains("비밀번호"))
                {
                    GuideForIncorrectlyEnteredData(imagePW, message);
                }
                else
                {
                    GuideForIncorrectlyEnteredData(imagePW, message);
                }

            }

        });

    }
    
    private IEnumerator LoginProcess()
    {
        float time = 0;

        while ( true )
        {
            time += Time.deltaTime;

            SetMessage($"로그인 중입니다... {time:F1}");

            yield return null;
        }
    }

}