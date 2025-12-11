using UnityEngine;

namespace SpacetimeSwap.Core
{
    public class TimeDeviceToggle : MonoBehaviour
    {
        [Header("References")]
        public GameObject timeDeviceRoot;   // 时间仪根物体（TimeDevice）
        public Camera otherWorldCamera;     // OtherWorldCamera

        [Header("Input")]
        public KeyCode toggleKey = KeyCode.Q;

        private bool _isActive = false;

        void Start()
        {
            ApplyState(_isActive);
        }

        void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                _isActive = !_isActive;
                ApplyState(_isActive);
            }
        }

        private void ApplyState(bool active)
        {
            if (timeDeviceRoot != null)
                timeDeviceRoot.SetActive(active);

            if (otherWorldCamera != null)
                otherWorldCamera.enabled = active;
        }
    }
}
