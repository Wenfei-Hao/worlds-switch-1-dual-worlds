using UnityEngine;

namespace SpacetimeSwap.Core
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController3D : MonoBehaviour
    {
        [Header("Move Settings")]
        public float moveSpeed = 6f;
        public float gravity = -9.81f;
        public float jumpHeight = 1.5f;

        [Header("Look Settings")]
        public Transform cameraRoot;     // 拖 CameraRoot 进来
        public float mouseSensitivity = 2f;
        public float minPitch = -80f;
        public float maxPitch = 80f;

        [Header("Animation")]
        public Animator animator;        // 拖模型上的 Animator 进来（或者用自动查找）
        public string runParamName = "IsRun";
        public string jumpParamName = "IsJump";

        private CharacterController _controller;
        private Vector3 _velocity;       // 包含垂直速度
        private float _pitch;

        // 给动画计算用
        private float _horizontalSpeed;
        private bool _isGrounded;

        void Awake()
        {
            _controller = GetComponent<CharacterController>();

            if (cameraRoot == null)
            {
                Debug.LogError("PlayerController3D: cameraRoot 未设置！");
            }

            // 如果没手动赋值，就自动找一个子物体里的 Animator
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            HandleLook();
            HandleMove();
            HandleAnimation();
        }

        private void HandleLook()
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            // 水平转身：转 PlayerRoot
            transform.Rotate(Vector3.up * mouseX * mouseSensitivity);

            // 垂直视角：转 cameraRoot
            _pitch -= mouseY * mouseSensitivity;
            _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

            if (cameraRoot != null)
            {
                cameraRoot.localEulerAngles = new Vector3(_pitch, 0f, 0f);
            }
        }

        private void HandleMove()
        {
            float h = Input.GetAxis("Horizontal"); // A/D
            float v = Input.GetAxis("Vertical");   // W/S

            Vector3 move = transform.right * h + transform.forward * v;
            if (move.magnitude > 1f)
                move.Normalize();

            Vector3 horizontalVelocity = move * moveSpeed;
            _horizontalSpeed = new Vector3(horizontalVelocity.x, 0f, horizontalVelocity.z).magnitude;

            _isGrounded = _controller.isGrounded;

            if (_isGrounded)
            {
                _velocity.y = -1f; // 轻轻压在地面上

                if (Input.GetButtonDown("Jump"))
                {
                    _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                }
            }
            else
            {
                _velocity.y += gravity * Time.deltaTime;
            }

            Vector3 finalVelocity = horizontalVelocity + new Vector3(0f, _velocity.y, 0f);
            _controller.Move(finalVelocity * Time.deltaTime);
        }

        private void HandleAnimation()
        {
            if (animator == null) return;

            // 跑步：在地面 & 有明显水平速度
            bool isRunning = _isGrounded && _horizontalSpeed > 0.1f;

            // 跳跃 / 空中：简单处理为“离地就算 Jump”
            bool isJumping = !_isGrounded;

            animator.SetBool(runParamName, isRunning);
            animator.SetBool(jumpParamName, isJumping);
        }

        // === 给 TimeShiftController 用的接口（保持不变） ===
        public Vector3 GetVelocity()
        {
            return _velocity;
        }

        public void SetVerticalVelocity(float y)
        {
            _velocity.y = y;
        }
    }
}
