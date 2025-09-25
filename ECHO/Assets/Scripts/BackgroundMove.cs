using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    public Transform player;          // 플레이어 Transform
    [Range(0f, 1f)]
    public float parallaxFactor = 0.3f; // 0~1 사이 값. 낮을수록 더 멀리 있는 느낌
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        float distanceX = player.position.x * parallaxFactor;
        float distanceY = player.position.y * parallaxFactor;

        // 배경은 X, Y 둘 다 느리게 따라옴 → 멀리 있는 풍경 느낌
        transform.position = new Vector3(startPosition.x + distanceX, startPosition.y + distanceY, startPosition.z);
    }
}
