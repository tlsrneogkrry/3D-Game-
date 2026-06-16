using UnityEngine;

public class SpinningObstacle : MonoBehaviour
{
    [Header("Rotation Settings")]
    public Vector3 rotationAxis = Vector3.up; // Y축 기준 회전 (기본)
    public float rotationSpeed = 90f;         // 초당 90도

    [Header("Knockback")]
    public float knockbackForce = 10f;
    public float knockbackUpForce = 5f;

    void Update()
    {
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
        if (rb == null) return;

        // 충돌 방향으로 튕겨내기
        Vector3 dir = (collision.transform.position - transform.position).normalized;
        dir.y = 0f;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce((dir * knockbackForce + Vector3.up * knockbackUpForce), ForceMode.VelocityChange);
    }
}