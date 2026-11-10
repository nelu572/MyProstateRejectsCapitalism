using UnityEngine;
using UnityEngine.InputSystem;
using MyProstateRejectsCapitalism.UI;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using MyProstateRejectsCapitalism.Gameplay;
#endif

namespace MyProstateRejectsCapitalism.Characters
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class FirstPersonPlayer : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private Camera viewCamera;
        [SerializeField, Min(0f)] private float moveSpeed = 3.5f;
        [SerializeField, Min(0f)] private float mouseSensitivity = 0.12f;
        [SerializeField, Min(.1f)] private float gravity = 20f;
        [SerializeField, Range(1f, 89f)] private float pitchLimit = 85f;
        [SerializeField] private bool useMovementTuning;
        [SerializeField, Min(.1f)] private float acceleration = 28f;
        [SerializeField, Min(.1f)] private float deceleration = 36f;
        [SerializeField, Min(.1f)] private float directionChangeAcceleration = 48f;
        [SerializeField, Range(0f, .3f)] private float groundSnapDistance = .18f;
        [SerializeField] private bool enableJump;
        [SerializeField, Min(.01f)] private float jumpHeight = .75f;

        private CharacterController controller;
        private InputActionAsset runtimeActions;
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction interactAction;
        private InputAction jumpAction;
        private Vector3 horizontalVelocity;
        private readonly RaycastHit[] groundHits = new RaycastHit[8];
        private float pitch;
        private float verticalVelocity;
        private int cursorLockedFrame;
        private int dialogueInputFrame = -1;
        private bool allowCursorCaptureWhileBlocked;
        private bool exhibitInspectionLocked;
        private CursorLockMode cursorBeforePause;

        public Camera ViewCamera => viewCamera;
        public bool InputBlocked { get; private set; }
        public bool DialogueLookAllowed { get; private set; }
        public bool IsPaused { get; private set; }
        public float MouseSensitivity => MenuSettings.Sensitivity(mouseSensitivity);
        public bool JumpEnabled { get => enableJump; set => enableJump = value; }
        public Vector3 HorizontalVelocity => horizontalVelocity;
        public bool NeedsCursorCapture => !IsPaused && isActiveAndEnabled && (!InputBlocked || allowCursorCaptureWhileBlocked)
            && Cursor.lockState != CursorLockMode.Locked;
        public bool AcceptsDialogueInput => !IsPaused && Cursor.lockState == CursorLockMode.Locked
            && Time.frameCount > cursorLockedFrame && !EscapePressed;
        public bool CanControl => !IsPaused && isActiveAndEnabled && !InputBlocked && Cursor.lockState == CursorLockMode.Locked
            && Time.frameCount > cursorLockedFrame;

        private static bool EscapePressed => Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;

        public void SetInputBlocked(bool blocked, bool allowCursorCapture = false)
        {
            InputBlocked = blocked;
            allowCursorCaptureWhileBlocked = blocked && allowCursorCapture;
            if (blocked)
                horizontalVelocity = Vector3.zero;
            if (!blocked)
                DialogueLookAllowed = false;
            // Do not reuse the dialogue's closing E/click as gameplay input in the same frame.
            if (!blocked)
                cursorLockedFrame = Time.frameCount;
        }

        public void SetDialogueLookAllowed(bool allowed) => DialogueLookAllowed = allowed;

        internal void SetExhibitInspectionLock(bool locked)
        {
            exhibitInspectionLocked = locked;
            SetInputBlocked(locked);
            ResetMotion();
        }

        internal void SetPaused(bool paused)
        {
            if (paused == IsPaused)
                return;
            IsPaused = paused;
            if (paused)
            {
                cursorBeforePause = Cursor.lockState;
                horizontalVelocity = Vector3.zero;
                SetCursorLocked(false);
            }
            else
            {
                SetCursorLocked(cursorBeforePause == CursorLockMode.Locked);
                cursorLockedFrame = Time.frameCount;
            }
        }

        public void SetMovementTuning(float speed, float startAcceleration, float stopDeceleration,
            float turnAcceleration, float height, float gravityStrength)
        {
            useMovementTuning = true;
            moveSpeed = Mathf.Max(0f, speed);
            acceleration = Mathf.Max(.1f, startAcceleration);
            deceleration = Mathf.Max(.1f, stopDeceleration);
            directionChangeAcceleration = Mathf.Max(.1f, turnAcceleration);
            jumpHeight = Mathf.Max(.01f, height);
            gravity = Mathf.Max(.1f, gravityStrength);
            horizontalVelocity = Vector3.zero;
        }

        public void ResetMotion()
        {
            horizontalVelocity = Vector3.zero;
            verticalVelocity = 0f;
        }

        // Sample one press for both bindings. Cursor capture and dialogue reserve their input frame.
        public bool InteractionPressedThisFrame => CanControl && !EscapePressed
            && Time.frameCount != dialogueInputFrame
            && (interactAction.WasPressedThisFrame() || Mouse.current?.leftButton.wasPressedThisFrame == true);

        internal void ReserveDialogueInputFrame() => dialogueInputFrame = Time.frameCount;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (viewCamera == null)
                viewCamera = GetComponentInChildren<Camera>();

            if (inputActions == null || viewCamera == null)
            {
                Debug.LogError("FirstPersonPlayer requires an input action asset and a child camera.", this);
                enabled = false;
                return;
            }

            // Each player owns its action state without enabling or modifying the project-wide asset.
            runtimeActions = Instantiate(inputActions);
            runtimeActions.bindingMask = InputBinding.MaskByGroup("Keyboard&Mouse");
            var playerMap = runtimeActions.FindActionMap("Player", true);
            moveAction = playerMap.FindAction("Move", true);
            lookAction = playerMap.FindAction("Look", true);
            interactAction = playerMap.FindAction("Interact", true);
            jumpAction = playerMap.FindAction("Jump", false);
            pitch = Mathf.DeltaAngle(0f, viewCamera.transform.localEulerAngles.x);
        }

        private void OnEnable()
        {
            if (runtimeActions == null)
                return;

            moveAction.Enable();
            lookAction.Enable();
            interactAction.Enable();
            jumpAction?.Enable();
            SetCursorLocked(true);
        }

        private void OnDisable()
        {
            if (runtimeActions != null)
                runtimeActions.Disable();
            ResetMotion();
            SetCursorLocked(false);
        }

        private void OnDestroy()
        {
            if (runtimeActions != null)
                Destroy(runtimeActions);
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused)
                SetCursorLocked(false);
        }

        private void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.ctrlKey.isPressed && keyboard.shiftKey.isPressed
                && keyboard.f9Key.wasPressedThisFrame && SliceActFlow.TryJumpToFinalToilet(this))
                return;
#endif
            if (IsPaused || exhibitInspectionLocked)
                return;
            if (EscapePressed)
            {
                if (!TryGetComponent<PauseMenu>(out _))
                    SetCursorLocked(false);
            }
            else if (NeedsCursorCapture && Mouse.current != null
                && Mouse.current.leftButton.wasPressedThisFrame)
                SetCursorLocked(true);

            var movement = Vector3.zero;
            bool grounded = controller.isGrounded;
            if (CanControl || (InputBlocked && DialogueLookAllowed && Cursor.lockState == CursorLockMode.Locked))
            {
                var look = lookAction.ReadValue<Vector2>() * MouseSensitivity;
                transform.Rotate(0f, look.x, 0f);
                pitch = Mathf.Clamp(pitch - look.y, -pitchLimit, pitchLimit);
                viewCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

            }

            if (CanControl)
            {
                var input = Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);
                var desired = (transform.right * input.x + transform.forward * input.y) * moveSpeed;
                if (useMovementTuning)
                {
                    float rate = desired.sqrMagnitude < .0001f ? deceleration : acceleration;
                    if (desired.sqrMagnitude > .0001f && horizontalVelocity.sqrMagnitude > .0001f
                        && Vector3.Dot(desired.normalized, horizontalVelocity.normalized) < .95f)
                        rate = directionChangeAcceleration;
                    horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, desired, rate * Time.deltaTime);
                    movement = horizontalVelocity;
                }
                else
                {
                    horizontalVelocity = desired;
                    movement = desired;
                }
            }
            else
                horizontalVelocity = Vector3.zero;

            if (grounded && verticalVelocity < 0f)
                verticalVelocity = -2f;
            if (CanControl && enableJump && grounded && jumpAction != null && jumpAction.WasPressedThisFrame())
                verticalVelocity = Mathf.Sqrt(2f * Mathf.Max(.1f, gravity) * jumpHeight);
            float previousVerticalVelocity = verticalVelocity;
            verticalVelocity = Mathf.Max(verticalVelocity - gravity * Time.deltaTime, -50f);
            movement.y = useMovementTuning
                ? (previousVerticalVelocity + verticalVelocity) * .5f
                : verticalVelocity;
            var collisions = controller.Move(movement * Time.deltaTime);
            if ((collisions & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
                verticalVelocity = 0f;
            if (useMovementTuning && grounded && verticalVelocity <= 0f && !controller.isGrounded)
                SnapToGround();
            if (useMovementTuning && controller.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -2f;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        internal void PlaceForFinalToiletTest(Vector3 position, Vector3 target)
        {
            controller.enabled = false;
            transform.position = position;
            var direction = target - viewCamera.transform.position;
            transform.rotation = Quaternion.Euler(0f, Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg, 0f);
            pitch = -Mathf.Atan2(direction.y, new Vector2(direction.x, direction.z).magnitude) * Mathf.Rad2Deg;
            viewCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            ResetMotion();
            controller.enabled = true;
            SetCursorLocked(true);
        }
#endif

        private void SnapToGround()
        {
            if (groundSnapDistance <= 0f)
                return;
            float radius = Mathf.Max(.01f, controller.radius - controller.skinWidth);
            float gap = .05f + controller.skinWidth;
            var feet = transform.TransformPoint(controller.center) - Vector3.up * (controller.height * .5f);
            var origin = feet + Vector3.up * (controller.radius + .05f);
            int count = Physics.SphereCastNonAlloc(origin, radius, Vector3.down, groundHits,
                gap + groundSnapDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            float nearest = float.PositiveInfinity;
            float minNormalY = Mathf.Cos(controller.slopeLimit * Mathf.Deg2Rad);
            for (int i = 0; i < count; i++)
            {
                var hit = groundHits[i];
                if (hit.collider == controller || hit.collider.transform.IsChildOf(transform)
                    || hit.normal.y < minNormalY)
                    continue;
                nearest = Mathf.Min(nearest, hit.distance);
            }
            if (!float.IsPositiveInfinity(nearest))
                controller.Move(Vector3.down * Mathf.Max(0f, nearest - gap));
        }

        private void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
            if (locked)
                cursorLockedFrame = Time.frameCount;
        }
    }
}
