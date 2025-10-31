using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DailyRankData : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI textRank;
    [SerializeField]
    private TextMeshProUGUI textDate;
    [SerializeField]
    private TextMeshProUGUI textMessage;

    private int rank;
    private int date;
    private string message;

    public int Rank
    {
        set
        {
            if (value <= Constants.MAK_RANK_LIST)
            {
                rank = value;
                textRank.text = rank.ToString();
            }
            else
            {
                textRank.text = "순위에 없음";
            }
        }
        get => rank;
    }

    public int Date
    {
        set
        {
            date = value;
            textDate.text = date.ToString();
        }
        get => date;
    }

    public string Message
    {
        set
        {
            message = value;
            textMessage.text = message;
        }
        get => message;
    }

}
