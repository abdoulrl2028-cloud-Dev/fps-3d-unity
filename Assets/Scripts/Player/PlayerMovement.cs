using UnityEngine;

namespace FPS.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 6f;
        [SerializeField] private float runSpeed = 11f;
        [SerializeField] private float jumpSpeed = 8f;
        [SerializeField] private float gravity = 20f;

        private CharacterController controller;
        private Vector3 velocity;

        public bool CanMove { get; set; } = true;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (!CanMove)
                return;

            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            bool run = Input.GetKey(KeyCode.LeftShift);

            Vector3 input = (Vector3.right * horizontal + Vector3.forward * vertical).normalized;
            Vector3 worldDir = (transform.right * input.x + transform.forward * input.z) * (run ? runSpeed : walkSpeed);

            if (controller.isGrounded)
            {
                velocity.y = -2f;
                if (Input.GetButtonDown("Jump"))
                    velocity.y = jumpSpeed;
            }
            else
            {
                velocity.y -= gravity * Time.deltaTime;
            }

            controller.Move((worldDir + Vector3.up * velocity.y) * Time.deltaTime);
        }

        public void TeleportTo(Vector3 position)
        {
            controller.enabled = false;
            transform.position = position;
            controller.enabled = true;
        }
    }
}