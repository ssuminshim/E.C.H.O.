using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameData
{
    // 게임이 실행되는 동안 이 변수는 파괴되지 않고 값을 유지
    // -1 = 미지정 (새 게임), 0 = Stage 1, 3 = Stage 4 ...
    public static int StageToReload = -1; 

    // Memory 씬(기억보관장치)을 완료했는지 여부
    public static bool HasCompletedMemory = false;
}