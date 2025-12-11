using UnityEngine;

namespace SpacetimeSwap.Core
{
    [RequireComponent(typeof(Camera))]
    public class OtherWorldCameraController : MonoBehaviour
    {
        [Header("References")]
        public Camera mainCamera;       // MainCamera
        public float worldOffsetY = 50f;

        private Camera _otherCam;

        void Awake()
        {
            _otherCam = GetComponent<Camera>();
        }

        void Start()
        {
            if (mainCamera == null)
            {
                Debug.LogError("OtherWorldCameraController: mainCamera 未设置！");
                enabled = false;
                return;
            }

            SyncCameraParams();
        }

        void LateUpdate()
        {
            if (TimeShiftController.Instance == null) return;

            // 1. 同步视锥参数（FOV、裁剪面、宽高比）
            SyncCameraParams();

            // 2. 同步位置 + 旋转 + worldOffset
            Transform mainT = mainCamera.transform;
            Vector3 pos = mainT.position;

            if (TimeShiftController.Instance.currentWorld == WorldType.WorldA)
            {
                // 玩家在 A，看 B
                pos.y += worldOffsetY;
            }
            else
            {
                // 玩家在 B，看 A
                pos.y -= worldOffsetY;
            }

            transform.position = pos;
            transform.rotation = mainT.rotation;
        }

        private void SyncCameraParams()
        {
            _otherCam.fieldOfView    = mainCamera.fieldOfView;
            _otherCam.nearClipPlane  = mainCamera.nearClipPlane;
            _otherCam.farClipPlane   = mainCamera.farClipPlane;
            _otherCam.aspect         = mainCamera.aspect;
        }
    }
}


/*
using UnityEngine;

namespace SpacetimeSwap.Core
{
    [RequireComponent(typeof(Camera))]
    public class OtherWorldCameraController : MonoBehaviour
    {
        [Header("References")]
        public Transform playerCamera;    // 玩家当前视角的相机（MainCamera）

        [Header("World Settings")]
        public float worldOffsetY = 50f;  // 与 TimeShiftController.worldOffsetY 一致

        private Camera _cam;

        void Awake()
        {
            _cam = GetComponent<Camera>();
        }

        void LateUpdate()
        {
            if (playerCamera == null || TimeShiftController.Instance == null)
                return;

            // 1. 取得当前世界
            var currentWorld = TimeShiftController.Instance.currentWorld;

            // 2. 以玩家相机的位置为基准
            Vector3 targetPos = playerCamera.position;

            // 3. 加上 / 减去偏移，得到“另一世界”的对应位置
            if (currentWorld == WorldType.WorldA)
            {
                // 玩家在 A，看 B
                targetPos.y += worldOffsetY;
            }
            else
            {
                // 玩家在 B，看 A
                targetPos.y -= worldOffsetY;
            }

            // 4. 应用位置和旋转
            transform.position = targetPos;
            transform.rotation = playerCamera.rotation;
        }
    }
}
*/