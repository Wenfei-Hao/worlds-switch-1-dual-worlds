using UnityEngine;

namespace SpacetimeSwap.Core
{
    public enum WorldType
    {
        WorldA,
        WorldB
    }

    public class TimeShiftController : MonoBehaviour
    {
        public static TimeShiftController Instance { get; private set; }

        [Header("References")]
        public Transform playerRoot;           // 指向 PlayerRoot
        public PlayerController3D playerController; // 指向 PlayerController3D

        [Header("World Settings")]
        public float worldOffsetY = 50f;       // 世界间的垂直距离
        public WorldType currentWorld = WorldType.WorldA;

        [Header("Input")]
        public KeyCode toggleKey = KeyCode.F;

        private CharacterController _characterController;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            if (playerRoot == null)
            {
                Debug.LogError("TimeShiftController: playerRoot 未设置！");
                enabled = false;
                return;
            }

            _characterController = playerRoot.GetComponent<CharacterController>();
            if (playerController == null)
            {
                playerController = playerRoot.GetComponent<PlayerController3D>();
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleWorld();
            }
        }

        public void ToggleWorld()
        {
            if (playerRoot == null) return;

            // 记录当前竖直速度，切换后保持下落 / 上升趋势
            float verticalVel = 0f;
            if (playerController != null)
            {
                verticalVel = playerController.GetVelocity().y;
            }

            Vector3 pos = playerRoot.position;

            if (currentWorld == WorldType.WorldA)
            {
                pos.y += worldOffsetY;
                currentWorld = WorldType.WorldB;
            }
            else
            {
                pos.y -= worldOffsetY;
                currentWorld = WorldType.WorldA;
            }

            // 安全地 Teleport：临时禁用 CharacterController
            if (_characterController != null)
            {
                _characterController.enabled = false;
            }

            playerRoot.position = pos;

            if (_characterController != null)
            {
                _characterController.enabled = true;
            }

            // 恢复竖直速度
            if (playerController != null)
            {
                playerController.SetVerticalVelocity(verticalVel);
            }

            Debug.Log($"TimeShift: 切换到 {currentWorld}, 新位置: {playerRoot.position}");
        }
    }
}
