using UnityEngine;

// 플레이어 하위 빈 오브젝트에 붙이거나, 바닥 아래 트리거 판정용 오브젝트에 붙이세요
// 방법 A: 플레이어에 직접 부착 → Y좌표 감시
// 방법 B: 바닥 아래에 큰 트리거 박스 배치 → OnTriggerEnter

public class FallDetector : MonoBehaviour
{
    [Header("Fall Settings")]
    [Tooltip("이 Y값 아래로 떨어지면 게임오버")]
    public float fallYThreshold = -10f;

    private bool triggered = false;

    void Update()
    {
        if (triggered) return;

        // 플레이어 자신에 붙어있는 경우 Y좌표로 감지
        if (transform.position.y < fallYThreshold)
        {
            TriggerGameOver();
        }
    }

    // 바닥 아래 트리거 박스에 붙이는 경우
    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (other.CompareTag("Player"))
        {
            TriggerGameOver();
        }
    }

    void TriggerGameOver()
    {
        triggered = true;
        if (GameManager.Instance != null)
            GameManager.Instance.TriggerGameOver();
    }
}