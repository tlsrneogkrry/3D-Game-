using UnityEngine;

public class GoalZone : MonoBehaviour
{
    [Header("Goal Settings")]
    [Tooltip("트리거 안에 들어오면 즉시 클리어 (true) / 점수 조건 충족 시에만 클리어 (false)")]
    public bool instantClear = true;

    [Header("Visual")]
    public Color goalColor = new Color(1f, 0.85f, 0f, 0.4f); // 반투명 황금색
    public float pulseSpeed = 2f;

    private Renderer rend;
    private bool triggered = false;

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
            rend.material.color = goalColor;
    }

    void Update()
    {
        // 골 존 반짝이는 연출
        if (rend != null && !triggered)
        {
            float alpha = Mathf.Abs(Mathf.Sin(Time.time * pulseSpeed)) * 0.5f + 0.2f;
            Color c = goalColor;
            c.a = alpha;
            rend.material.color = c;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        if (instantClear)
        {
            // 즉시 클리어
            TriggerClear();
        }
        else
        {
            // 점수 조건 확인 후 클리어
            if (GameManager.Instance != null && GameManager.Instance.Score >= GameManager.Instance.ClearScore)
                TriggerClear();
            else
                ShowNotEnoughScore();
        }
    }

    void TriggerClear()
    {
        triggered = true;

        if (rend != null)
        {
            Color c = rend.material.color;
            c.a = 1f;
            rend.material.color = c;
        }

        if (GameManager.Instance != null)
            GameManager.Instance.TriggerGameClear();
    }

    void ShowNotEnoughScore()
    {
        int need = GameManager.Instance.ClearScore - GameManager.Instance.Score;
        Debug.Log($"[GoalZone] 아직 {need}점 부족합니다!");
        // GameUI에 메시지 표시하고 싶으면 GameUI.Instance.ShowHint($"{need}점 더 모으세요!"); 추가
    }
}