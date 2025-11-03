using UnityEngine;

// 이 스크립트는 Core 씬의 Main Camera에 붙어있어야 합니다.
[RequireComponent(typeof(Camera))]
public class CameraClamp : MonoBehaviour
{
    [Header("추적 대상")]
    public Transform target; // 1. 인스펙터에서 Player를 연결
    public Vector3 offset = new Vector3(0, 0, -10); // 카메라 오프셋

    [Header("속도 (경계 없을 때)")]
    public float moveSpeed = 8f; // 카메라가 부드럽게 따라가는 속도

    // 맵의 경계 (매 스테이지마다 새로 찾음)
    private BoxCollider2D bound;
    private Vector3 minBound;
    private Vector3 maxBound;
    
    // 카메라의 화면 크기 (경계 계산용)
    private Camera cam;
    private float camHeight;
    private float camWidth;

    void Awake()
    {
        cam = GetComponent<Camera>();

        // 혹시 Target이 연결 안됐으면 GameManager에서 Player를 찾아옴
        if (target == null && GameManager.Instance != null && GameManager.Instance.player != null)
        {
            target = GameManager.Instance.player.transform;
        }
    }

    // GameManager가 스테이지 로드 직후 이 함수를 호출할 것입니다.
    public void FindNewBoundary()
    {
        // 1. "CameraBoundary" 태그를 가진 새 경계 콜라이더를 찾음
        GameObject boundaryObject = GameObject.FindWithTag("CameraBoundary");
        if (boundaryObject != null)
        {
            bound = boundaryObject.GetComponent<BoxCollider2D>();
            
            // 2. 콜라이더로부터 경계값 계산 (Tistory 링크 로직)
            minBound = bound.bounds.min;
            maxBound = bound.bounds.max;

            // 3. 카메라의 세로/가로 절반 크기 계산
            camHeight = cam.orthographicSize;
            camWidth = camHeight * cam.aspect; // aspect = 화면 비율
        }
        else
        {
            bound = null; // 이 씬에는 경계가 없음
            Debug.LogWarning(gameObject.scene.name + " 씬에서 CameraBoundary 태그를 가진 오브젝트를 찾을 수 없습니다.");
        }
    }
    
    // Tistory 링크와 동일하게 LateUpdate 사용 (플레이어가 움직인 '후'에 카메라가 움직임)
    void LateUpdate()
    {
        // 타겟(플레이어)이 없으면 아무것도 안 함
        if (target == null) return;

        // 1. 카메라가 따라갈 목표 위치 계산
        Vector3 desiredPosition = target.position + offset;

        // 2. 경계(bound)가 설정되어 있을 때만 위치 제한
        if (bound != null)
        {
            // 3. 카메라의 위치를 경계값으로 제한 (Tistory 로직)
            // 카메라의 '중앙'이 경계(min/max)에서 '카메라 절반 크기'만큼 떨어진 곳을 넘지 못하게 함
            float clampedX = Mathf.Clamp(desiredPosition.x, minBound.x + camWidth, maxBound.x - camWidth);
            float clampedY = Mathf.Clamp(desiredPosition.y, minBound.y + camHeight, maxBound.y - camHeight);

            // 4. 최종 위치 설정
            // (부드럽게 이동)
            transform.position = Vector3.Lerp(transform.position, new Vector3(clampedX, clampedY, offset.z), moveSpeed * Time.deltaTime);
            // (즉시 이동)
            // transform.position = new Vector3(clampedX, clampedY, offset.z);
        }
        else
        {
            // 5. 경계가 없으면 (예: 메뉴 씬) 그냥 부드럽게 따라감
            transform.position = Vector3.Lerp(transform.position, desiredPosition, moveSpeed * Time.deltaTime);
        }
    }
}