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
                // GameManager에게 카메라 연출을 시작하라고 명령
                GameManager.Instance.StartCreditPan(cameraPanTarget);
            }
            
            // 이 트리거는 비활성화
            gameObject.SetActive(false);
        }
    }
}