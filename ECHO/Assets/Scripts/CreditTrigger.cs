// CreditTrigger.cs
using UnityEngine;

public class CreditTrigger : MonoBehaviour
{
    // 인스펙터에서 카메라가 이동할 목표 지점을 연결
    public Transform cameraPanTarget; 
    
    private bool hasTriggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어가 닿았고, 아직 발동된 적 없다면
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true; // 중복 실행 방지
            
            if (cameraPanTarget != null)
            {
                // 다시 연출 시작 함수를 호출
                // 이 함수가 내부적으로 연출 후 NextStage()를 부를 것임
                GameManager.Instance.StartCreditPan(cameraPanTarget);
            }
            else
            {
                // 만약 타겟이 없다면 그냥 다음 스테이지로 넘어감 (안전장치)
                GameManager.Instance.NextStage();
            }
            
            // 이 트리거는 비활성화
            gameObject.SetActive(false);
        }
    }
}