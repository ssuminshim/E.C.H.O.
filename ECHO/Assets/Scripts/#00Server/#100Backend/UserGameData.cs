[System.Serializable]
public class UserGameData
{
    public string   Message; // 플레이어 메시지
    public int      Date;    // 플레이어 메시지 등록 시간

    public void Reset()
    {
        Message = "이것은 메시지입니다.";
        Date = 0;
    }
}
