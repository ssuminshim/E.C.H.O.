using UnityEngine;

public class Stage : MonoBehaviour
{
    public Transform spawnPoint;

    void Start()
    {
        // 씬이 로드될 때 싱글톤 GameManager에게 자신을 등록
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterStage(this);
        }
        else
        {
            Debug.LogError("GameManager.Instance를 찾을 수 없습니다! 'Core' 씬이 먼저 로드되었는지 확인하세요.");
        }
    }
}