using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BackEnd;
using UnityEngine.SceneManagement;

public class RegisterAccount : LoginBass
{
    [SerializeField]
    private Image imageID; // ID 필드 색상 변경

    [SerializeField]
    private TMP_InputField inputFieldID; // ID 필드 텍스트 정보 추출

    [SerializeField]
    private Image imagePW; // PW 필드 색상 변경

    [SerializeField]
    private TMP_InputField inputFieldPW; // PW 필드 텍스트 정보 추출


    [SerializeField]
    private Button btnRegisterAccount; // PW 필드 텍스트 정보 추출

    // "계정 생성" 버튼 눌렀을 때 호출
    public void OnClickRegisterAcccount()
    {
        // 매개변수로 입력한 inputfield UI의 색상과 message 내용 초기화
        ResetUI(imageID, imagePW);

        // 필드 값이 비어있는지 체크
        if ( IsFieldDataEmpty(imageID, inputFieldID.text, "아이디")) return;
        if  (IsFieldDataEmpty(imagePW, inputFieldPW.text, "비밀번호")) return;

        // 계정 생성 버튼의 상호작용 비활성화
        btnRegisterAccount.interactable = false;
        SetMessage("계정 생성중입니다..");

        // 뒤끝 서버 계정 생성 시도
        CustomSignUP();
    }

    // 계정 생성 시도 후 서버로부터 전달받은 message를 기반으로 로직 처리
    private void CustomSignUP()
    {
        Backend.BMember.CustomSignUp(inputFieldID.text, inputFieldPW.text, callback =>
        {
            //"계정 생성" 버튼 상호작용 활성화
            btnRegisterAccount.interactable = true;

            // 계정 생성 성공
            if (callback.IsSuccess())
            {
                SetMessage($"계정 생성 성공. {inputFieldID.text}님 환영합니다.");

                // 계정 생성에 성공했을 때 해당 계정의 게임 정보 생성
                BackendGameData.Instance.GameDataInsert();

                // Memory 씬으로 이동
                SceneManager.LoadScene("Memory");
            }
            // 게정 생성 실패
            else
            {
                string message = string.Empty;

                switch (int.Parse(callback.GetStatusCode()))
                {
                    case 409: // 중복된 customID 존재
                        message = "이미 존재하는 아이디입니다.";
                        break;
                    case 404: // 차단당한 디바이스
                    case 401: // 프로젝트 상태가 '점검'일 경우
                    case 400: // 디바이스 정보가 null일 경우
                    default:
                        message = callback.GetMessage();
                        break;
                }

                if (message.Contains("아이디"))
                {
                    GuideForIncorrectlyEnteredData(imageID, message);
                }
                else
                {
                    SetMessage(message);
                }
            }
        });

    }

}
