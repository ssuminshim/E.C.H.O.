using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoginBass : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI textMessage;

    //메시지 내용, InputField 색상 초기화

    protected void ResetUI(params Image[] images)
    {
        textMessage.text = string.Empty;

        for (int i = 0; i < images.Length; ++i)
        {
            images[i].color = Color.white;
        }
    }

    // 매개변수에 있는 내용을 출력
    protected void SetMessage(string msg)
    {
        textMessage.text = msg;
    }

    protected void GuideForIncorrectlyEnteredData(Image image, string msg)
    {
        textMessage.text = msg;
        image.color = Color.red;
    }

    // 

    protected bool IsFieldDataEmpty(Image image, string field, string result)
    {
        if (field.Trim().Equals(""))
        {
            GuideForIncorrectlyEnteredData(image, $"\"{result}\" 필드를 채워주세요.");

            return true;
        }

        return false;
    }
}
