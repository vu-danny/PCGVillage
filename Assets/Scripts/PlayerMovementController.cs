using UnityEngine;

namespace Main
{
    public class PlayerMovementController : MonoBehaviour
    {
        [SerializeField] private CharacterController controller;

        [Header("Horizontal Movement Modifiers")]
        private Vector3 targetHorizontalVelocity;
        private Vector3 horizontalVelocity;
        [SerializeField] private float _speed = 8f;
        public float Speed
        {
            get { return _speed; }
            set { _speed = value; }
        }
        [SerializeField] private float groundedMovementModifier = 1f;
        [SerializeField] private float airboneMovementModifier = 0.5f;
        [SerializeField] private float diagonalMovementAmplifier = 0.075f;

        [Header("Vertical Movement Modifiers")]
        [SerializeField] private float gravity = -9.81f * 2;
        [SerializeField] private float jumpHeight;
        [SerializeField] private float jumpInputBufferTime = 0.125f;
        private float currentJumpInputTime;

        private Vector3 verticalVelocity = new Vector3();
        private bool isGrounded;
        private float baseStepOffset;

        private void Awake()
        {
            baseStepOffset = controller.stepOffset;
        }

        private void Update()
        {
            if (Input.GetButtonDown("Jump"))
                currentJumpInputTime = jumpInputBufferTime;

            ComputeTargetHorizontalVelocity();
            ComputeMovement();
        }

        private void FixedUpdate()
        {
            float horizontalMovementModifier = isGrounded ? groundedMovementModifier : airboneMovementModifier;

            Vector3 actualHorizontalVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);
            Vector3 horizontalVelocityChange = targetHorizontalVelocity - actualHorizontalVelocity;
            horizontalVelocity = actualHorizontalVelocity + horizontalVelocityChange * Time.fixedDeltaTime * horizontalMovementModifier;
        }

        private void ComputeMovement()
        {
            isGrounded = (controller.isGrounded) && (verticalVelocity.y <= 0);
            ComputeVerticalVelocity();
            ComputeVerticalSlopeSlide();
            controller.Move((horizontalVelocity + verticalVelocity) * Time.deltaTime);
        }

        private void ComputeTargetHorizontalVelocity()
        {
            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");

            Vector3 movementDirection = transform.right * x + transform.forward * z;
            movementDirection = Vector3.ClampMagnitude(movementDirection, 1 + diagonalMovementAmplifier);
            targetHorizontalVelocity = movementDirection * _speed;
        }

        private void ComputeVerticalVelocity()
        {
            if (isGrounded && verticalVelocity.y < 0)
                verticalVelocity.y = -0.5f;

            if (currentJumpInputTime < 0)
                currentJumpInputTime = -1;

            if (currentJumpInputTime > 0 && isGrounded)
            {
                verticalVelocity.y = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(gravity));
                currentJumpInputTime = 0;
            }

            if (!isGrounded)
                controller.stepOffset = 0f;
            else
                controller.stepOffset = baseStepOffset;

            verticalVelocity.y += gravity * Time.deltaTime;
            currentJumpInputTime -= Time.deltaTime;
        }

        private void ComputeVerticalSlopeSlide()
        {
            if (isGrounded && verticalVelocity.y < 0)
            {
                RaycastHit hit1Info;
                float castDistance = controller.height * 1.25f;

                if (Physics.Raycast(transform.position, Vector3.down, out hit1Info, castDistance))
                {
                    // Position of the possible next position of player on the ground
                    // (at the player's feet)
                    Vector3 possibleNextGroundedPosition = hit1Info.point + horizontalVelocity * Time.deltaTime;

                    RaycastHit hit2Info;
                    if (Physics.Raycast(possibleNextGroundedPosition, Vector3.down, out hit2Info, castDistance) && (hit1Info.point.y - hit2Info.point.y) > 1e-6f)
                        verticalVelocity.y += (hit2Info.point.y - hit1Info.point.y) / Time.deltaTime;
                }
            }
        }
    }
}
