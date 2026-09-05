using UnityEngine;

namespace FPS.Player
{
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerLook : MonoBehaviour
    {
        [Header("Look")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float maxLookUp = -60f;
        [SerializeField] private float maxLookDown = 60f;
        [SerializeField] private bool invertedY = false;

        private float verticalRotation;

        public bool CanLook { get; set; } = true;

        private void Awake()
        {
            if (playerCamera == null)
                playerCamera = GetComponentInChildren<Camera>();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (!CanLook || playerCamera == null)
                return;

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * (invertedY ? 1f : -1f);

            transform.Rotate(0f, mouseX, 0f, Space.World);

            verticalRotation = Mathf.Clamp(verticalRotation + mouseY, maxLookUp, maxLookDown);
            playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }
    }
}