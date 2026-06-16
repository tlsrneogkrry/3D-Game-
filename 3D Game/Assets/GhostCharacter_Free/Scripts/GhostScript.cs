using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Sample
{
    public class GhostScript : MonoBehaviour
    {
        // ─── Components ───────────────────────────────────────────
        private Animator Anim;
        private CharacterController Ctrl;
        private ThirdPersonCamera CamScript; // 카메라 스크립트 참조

        // ─── Animator state hashes ────────────────────────────────
        private static readonly int IdleState = Animator.StringToHash("Base Layer.idle");
        private static readonly int MoveState = Animator.StringToHash("Base Layer.move");
        private static readonly int SurprisedState = Animator.StringToHash("Base Layer.surprised");
        private static readonly int AttackState = Animator.StringToHash("Base Layer.attack_shift");
        private static readonly int DissolveState = Animator.StringToHash("Base Layer.dissolve");
        private static readonly int AttackTag = Animator.StringToHash("Attack");

        // ─── Dissolve ─────────────────────────────────────────────
        [SerializeField] private SkinnedMeshRenderer[] MeshR;
        private float Dissolve_value = 1f;
        private bool DissolveFlg = false;

        // ─── HP ───────────────────────────────────────────────────
        private const int maxHP = 3;
        private int HP = maxHP;
        private Text HP_text;

        // ─── Movement ─────────────────────────────────────────────
        [SerializeField] private float Speed = 5f;
        [SerializeField] private float JumpForce = 6f;
        [SerializeField] private float Gravity = 20f;
        private float verticalVelocity = 0f;
        private float bounceTimer = 0f; // 바운스 후 접지 무시 시간

        // ─── Input ────────────────────────────────────────────────
        private Keyboard kb;
        private Gamepad gp;

        // ══════════════════════════════════════════════════════════
        void Start()
        {
            Anim = GetComponent<Animator>();
            Ctrl = GetComponent<CharacterController>();

            // Main Camera에 붙어있는 ThirdPersonCamera 스크립트를 자동으로 찾음
            if (Camera.main != null)
                CamScript = Camera.main.GetComponent<ThirdPersonCamera>();

            if (CamScript == null)
                Debug.LogError("[GhostScript] Main Camera에 ThirdPersonCamera 스크립트가 없습니다!");

            HP_text = GameObject.Find("Canvas/HP")?.GetComponent<Text>();
            if (HP_text != null) HP_text.text = "HP " + HP;
        }

        // ══════════════════════════════════════════════════════════
        void Update()
        {
            kb = Keyboard.current;
            gp = Gamepad.current;

            Move();
            Jump();
            ApplyGravity();
            Attack();
            DamageTest();
            Respawn();
            Dissolve();
        }

        // ══════════════════════════════════════════════════════════
        // WASD 이동 — 카메라 yaw 기준
        // ══════════════════════════════════════════════════════════
        void Move()
        {
            if (DissolveFlg) return;

            Vector2 input = Vector2.zero;
            if (kb != null)
            {
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) input.y += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) input.y -= 1f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) input.x -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) input.x += 1f;
            }
            if (gp != null) input += gp.leftStick.ReadValue();
            input = Vector2.ClampMagnitude(input, 1f);

            Vector3 moveVec = Vector3.zero;

            if (input.magnitude > 0.01f)
            {
                // 카메라 yaw(수평 회전)만 기준으로 이동 방향 계산
                Quaternion camRot = CamScript != null
                    ? CamScript.GetCameraRotation()
                    : Quaternion.identity;

                Vector3 forward = camRot * Vector3.forward;
                Vector3 right = camRot * Vector3.right;

                moveVec = (forward * input.y + right * input.x).normalized * Speed;

                // 이동 방향으로 캐릭터 회전
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(new Vector3(moveVec.x, 0f, moveVec.z)),
                    Time.deltaTime * 10f);

                // Move 애니메이션
                var info = Anim.GetCurrentAnimatorStateInfo(0);
                if (info.fullPathHash != MoveState && info.tagHash != AttackTag)
                    Anim.CrossFade(MoveState, 0.1f, 0, 0);
            }
            else
            {
                // Idle 애니메이션
                var info = Anim.GetCurrentAnimatorStateInfo(0);
                if (info.fullPathHash != IdleState &&
                    info.tagHash != AttackTag &&
                    info.fullPathHash != SurprisedState)
                    Anim.CrossFade(IdleState, 0.1f, 0, 0);
            }

            Ctrl.Move(new Vector3(moveVec.x, verticalVelocity, moveVec.z) * Time.deltaTime);
        }

        // ══════════════════════════════════════════════════════════
        // 점프
        // ══════════════════════════════════════════════════════════
        void Jump()
        {
            if (DissolveFlg) return;

            bool jumped = (kb != null && kb.spaceKey.wasPressedThisFrame)
                       || (gp != null && gp.buttonSouth.wasPressedThisFrame);

            if (jumped && IsGrounded())
                verticalVelocity = JumpForce;
        }

        // ══════════════════════════════════════════════════════════
        // 중력
        // ══════════════════════════════════════════════════════════
        void ApplyGravity()
        {
            if (bounceTimer > 0f)
            {
                bounceTimer -= Time.deltaTime;
                verticalVelocity -= Gravity * Time.deltaTime; // 공중에서 중력은 계속 적용
                return;
            }

            if (IsGrounded() && verticalVelocity < 0f)
                verticalVelocity = -2f;
            else
                verticalVelocity -= Gravity * Time.deltaTime;
        }

        // ── 바운스 패드에서 호출 ──────────────────────────────────
        public void ApplyBounce(float force)
        {
            verticalVelocity = force;   // 위로 속도 세팅
            bounceTimer = 0.3f;         // 0.3초간 접지 판정 무시
        }

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            // 발 아래 오브젝트가 BouncePad인지 확인
            if (hit.moveDirection.y < -0.3f)
            {
                BouncePad pad = hit.collider.GetComponent<BouncePad>();
                if (pad != null) pad.DoBounce(this);
            }
        }

        bool IsGrounded()
        {
            if (Ctrl.isGrounded) return true;
            return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.25f);
        }

        // ══════════════════════════════════════════════════════════
        // 공격 (J)
        // ══════════════════════════════════════════════════════════
        void Attack()
        {
            if (DissolveFlg) return;
            bool pressed = (kb != null && kb.jKey.wasPressedThisFrame)
                        || (gp != null && gp.buttonWest.wasPressedThisFrame);
            if (pressed) Anim.CrossFade(AttackState, 0.1f, 0, 0);
        }

        // ══════════════════════════════════════════════════════════
        // 데미지 테스트 (F)
        // ══════════════════════════════════════════════════════════
        void DamageTest()
        {
            if (DissolveFlg) return;
            if (kb == null || !kb.fKey.wasPressedThisFrame) return;
            Anim.CrossFade(SurprisedState, 0.1f, 0, 0);
            HP--;
            if (HP_text != null) HP_text.text = "HP " + HP;
        }

        // ══════════════════════════════════════════════════════════
        // 리스폰 (R)
        // ══════════════════════════════════════════════════════════
        void Respawn()
        {
            if (kb == null || !kb.rKey.wasPressedThisFrame) return;
            HP = maxHP;
            if (HP_text != null) HP_text.text = "HP " + HP;

            Ctrl.enabled = false;
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            Ctrl.enabled = true;

            verticalVelocity = 0f;
            Dissolve_value = 1f;
            DissolveFlg = false;
            for (int i = 0; i < MeshR.Length; i++)
                MeshR[i].material.SetFloat("_Dissolve", 1f);

            Anim.CrossFade(IdleState, 0.1f, 0, 0);
        }

        // ══════════════════════════════════════════════════════════
        // 디졸브
        // ══════════════════════════════════════════════════════════
        void Dissolve()
        {
            if (HP <= 0 && !DissolveFlg)
            {
                Anim.CrossFade(DissolveState, 0.1f, 0, 0);
                DissolveFlg = true;
            }
            if (DissolveFlg)
            {
                Dissolve_value = Mathf.Max(0f, Dissolve_value - Time.deltaTime);
                for (int i = 0; i < MeshR.Length; i++)
                    MeshR[i].material.SetFloat("_Dissolve", Dissolve_value);
                if (Dissolve_value <= 0f) Ctrl.enabled = false;
            }
        }
    }
}