using UnityEngine;

public class Stage : MonoBehaviour
{
    // 인스펙터 창에서 이 스테이지의 스폰 지점(빈 오브젝트)을 연결해주세요.
    public Transform spawnPoint;

    void Start()
    {
        // 씬이 로드될 때, 싱글톤 GameManager에게 자신을 등록합니다.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterStage(this);
        }
        else
        {
            Debug.LogError("GameManager.Instance를 찾을 수 없습니다! '#03Core' 씬이 먼저 로드되었는지 확인하세요.");
        }
    }
}