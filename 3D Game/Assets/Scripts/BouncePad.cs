using UnityEngine;
using System.Collections;

public class BouncePad : MonoBehaviour
{
    [Header("Bounce Settings")]
    public float bounceForce = 20f;
    public float scaleSquishAmount = 0.4f;
    public float squishDuration = 0.1f;

    [Header("Visual")]
    public Color normalColor = new Color(0.2f, 0.8f, 0.3f);
    public Color bounceColor = new Color(1f, 0.9f, 0.1f);

    private Vector3 originalScale;
    private Renderer padRenderer;
    private bool isAnimating = false;

    void Start()
    {
        originalScale = transform.localScale;
        padRenderer = GetComponent<Renderer>();
        if (padRenderer != null)
            padRenderer.material.color = normalColor;
    }

    // GhostScript의 OnControllerColliderHit에서 호출
    public void DoBounce(Sample.GhostScript ghost)
    {
        ghost.ApplyBounce(bounceForce);

        if (!isAnimating)
            StartCoroutine(BounceAnimation());
    }

    IEnumerator BounceAnimation()
    {
        isAnimating = true;
        if (padRenderer != null) padRenderer.material.color = bounceColor;

        Vector3 squished = new Vector3(
            originalScale.x * (1f + scaleSquishAmount),
            originalScale.y * (1f - scaleSquishAmount),
            originalScale.z * (1f + scaleSquishAmount));

        float t = 0f;
        while (t < squishDuration) { t += Time.deltaTime; transform.localScale = Vector3.Lerp(originalScale, squished, t / squishDuration); yield return null; }
        t = 0f;
        while (t < squishDuration) { t += Time.deltaTime; transform.localScale = Vector3.Lerp(squished, originalScale, t / squishDuration); yield return null; }

        transform.localScale = originalScale;
        if (padRenderer != null) padRenderer.material.color = normalColor;
        isAnimating = false;
    }
}