using UnityEngine;

namespace FPS.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class FpsPlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 6f;
        [SerializeField] private float runSpeed = 11f;
        [SerializeField] private float jumpSpeed = 8f;
        [SerializeField] private float gravity = 20f;
        [SerializeField] private float movementSmoothing = 10f;

        [Header("Camera / Look")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float maxLookUp = -60f;
        [SerializeField] private float maxLookDown = 60f;

        [Header("State")]
        [SerializeField] private bool canMove = true;

        private CharacterController controller;
        private Vector3 velocity;
        private Vector3 currentMovement;
        private Vector3 smoothMovement;
        private float verticalRotation;
        private bool alive = true;

        public bool CanMove { get => canMove; set => canMove = value; }
        public bool IsGrounded => controller.isGrounded;
        public CharacterController Controller => controller;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (playerCamera == null)
                playerCamera = GetComponentInChildren<Camera>();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (!canMove || !alive)
            {
                ApplyGravity();
                return;
            }

            HandleLook();
            HandleMovement();
        }

        private void HandleLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, maxLookUp, maxLookDown);

            transform.Rotate(Vector3.up * mouseX, Space.World);
            if (playerCamera != null)
                playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }

        private void HandleMovement()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            bool running = Input.GetKey(KeyCode.LeftShift);
            float speed = running ? runSpeed : walkSpeed;

            Vector3 input = (transform.right * horizontal + transform.forward * vertical);
            if (input.magnitude > 1f)
                input.Normalize();

            Vector3 targetVelocity = input * speed;
            currentMovement = Vector3.SmoothDamp(currentMovement, targetVelocity, ref smoothMovement, 1f / movementSmoothing);

            Vector3 motion = currentMovement * Time.deltaTime;
            motion.y = velocity.y * Time.deltaTime;
            controller.Move(motion);

            if (controller.isGrounded)
            {
                if (Input.GetButton("Jump"))
                    velocity.y = jumpSpeed;
                else if (velocity.y < 0f)
                    velocity.y = -1f;
            }
            else
            {
                velocity.y -= gravity * Time.deltaTime;
            }
        }

        private void ApplyGravity()
        {
            if (!controller.isGrounded)
                velocity.y -= gravity * Time.deltaTime;
            else if (velocity.y < 0f)
                velocity.y = -1f;

            controller.Move(new Vector3(0f, velocity.y * Time.deltaTime, 0f));
        }

        public void SetAlive(bool value)
        {
            alive = value;
            if (!alive)
                canMove = false;
        }

        public void Teleport(Vector3 position)
        {
            controller.enabled = false;
            transform.position = position;
            controller.enabled = true;
            velocity = Vector3.zero;
        }
    }
}