using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    [Header("Item Settings")]
    public int scoreValue = 10;
    public float rotationSpeed = 90f;
    public float bobSpeed = 2f;
    public float bobHeight = 0.3f;

    [Header("Collect Effect")]
    public GameObject collectEffectPrefab; // 파티클 등 (선택사항)

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // 아이템 회전 애니메이션
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // 위아래 둥실둥실 애니메이션
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void OnTriggerEnter(Collider other)
    {
        // 플레이어 태그 확인 (Player 태그를 플레이어에게 설정해야 함)
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    void Collect()
    {
        // GameManager에 점수 추가
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }

        // 이펙트 생성 (프리팹이 있을 경우)
        if (collectEffectPrefab != null)
        {
            Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);
        }

        // 아이템 제거
        Destroy(gameObject);
    }
}