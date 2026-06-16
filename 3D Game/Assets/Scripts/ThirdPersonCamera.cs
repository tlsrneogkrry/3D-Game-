using UnityEngine;
using UnityEngine.InputSystem;

namespace Sample
{
    /// <summary>
    /// Main Camera 오브젝트에 이 스크립트를 붙이세요.
    /// Inspector의 Target에 Ghost 오브젝트를 드래그하면 됩니다.
    /// </summary>
    public class ThirdPersonCamera : MonoBehaviour
    {
        [Header("따라갈 대상")]
        [SerializeField] private Transform Target;          // Ghost 오브젝트
        [SerializeField] private Vector3 TargetOffset = new Vector3(0f, 1.5f, 0f); // 눈 높이 오프셋

        [Header("카메라 거리 / 감도")]
        [SerializeField] private float Distance = 5f;
        [SerializeField] private float MouseSensitivity = 0.2f;
        [SerializeField] private float PitchMin = -15f;
        [SerializeField] private float PitchMax = 60f;
        [SerializeField] private float FollowSpeed = 10f; // 부드럽게 따라가는 속도

        private float yaw = 0f;
        private float pitch = 15f;
        private Mouse mouse;
        private Gamepad gp;

        void Start()
        {
            if (Target == null)
                Debug.LogError("[ThirdPersonCamera] Target이 비어 있습니다! Inspector에서 Ghost를 연결하세요.");

            // 초기 yaw를 타겟 방향으로
            if (Target != null)
                yaw = Target.eulerAngles.y;

            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
        }

        // 카메라는 LateUpdate에서 움직여야 캐릭터 이동 후 따라감
        void LateUpdate()
        {
            if (Target == null) return;

            mouse = Mouse.current;
            gp = Gamepad.current;

            // ── 마우스 / 게임패드 입력 ──────────────────────────
            Vector2 look = Vector2.zero;
            if (mouse != null) look += mouse.delta.ReadValue();
            if (gp != null) look += gp.rightStick.ReadValue() * 5f;

            yaw += look.x * MouseSensitivity;
            pitch -= look.y * MouseSensitivity;
            pitch = Mathf.Clamp(pitch, PitchMin, PitchMax);

            // ── 카메라 위치 계산 ────────────────────────────────
            Vector3 pivot = Target.position + TargetOffset;
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 desiredPos = pivot + rotation * new Vector3(0f, 0f, -Distance);

            // 벽 클리핑 방지 (캐릭터와 카메라 사이 장애물 체크)
            RaycastHit hit;
            float dist = Distance;
            if (Physics.Raycast(pivot, (desiredPos - pivot).normalized, out hit, Distance + 0.3f))
                dist = Mathf.Max(0.5f, hit.distance - 0.2f);

            Vector3 finalPos = pivot + rotation * new Vector3(0f, 0f, -dist);

            // 부드럽게 이동
            transform.position = Vector3.Lerp(transform.position, finalPos, Time.deltaTime * FollowSpeed);
            transform.LookAt(pivot);

            // Esc 커서 해제
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                UnityEngine.Cursor.lockState = CursorLockMode.None;
                UnityEngine.Cursor.visible = true;
            }
        }

        // 외부에서 yaw 값을 읽을 수 있도록 (GhostScript의 이동 방향 계산용)
        public float GetYaw() => yaw;
        public Quaternion GetCameraRotation() => Quaternion.Euler(0f, yaw, 0f);
    }
}