using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [Header("커서 이미지")]
    public Texture2D cursorTexture; // 1. 인스펙터에서 연결할 커서 이미지

    [Header("핫스팟 (클릭 지점)")]
    public Vector2 hotspot = Vector2.zero; // 2. 커서의 '클릭 지점' (0,0 = 좌측 상단)

    // 게임이 시작될 때 한 번만 실행
    void Start()
    {
        // 3. 커서를 설정합니다.
        // (이미지, 핫스팟, 모드)
        Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
    }

    // (선택) 게임이 꺼질 때 기본 커서로 되돌리기
    void OnApplicationQuit()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}