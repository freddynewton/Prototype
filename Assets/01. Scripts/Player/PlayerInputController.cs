using Game.Character;
using Game.Character.Camera;
using Game.Data.CharacterInputData;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Player
{
    public class PlayerInputController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private InputActionAsset inputActions;

        public CharacterMovement CharacterMovementController { get => characterMovementController; }
        [SerializeField]
        private CharacterMovement characterMovementController;

        public CharacterCameraController CharacterCameraController { get => characterCameraController; }
        [SerializeField]
        private CharacterCameraController characterCameraController;

        // Action Maps
        private InputActionMap playerActionMap;
        private InputActionMap uiActionMap;

        // Actions
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction primaryAction;
        private InputAction secondaryAction;
        private InputAction zoomAction;
        private InputAction jumpAction;

        // Input Values
        private Vector2 lookInput;
        private Vector2 moveInput;
        private float zoomInput;
        private bool jumpInput;

        // State
        private bool isInitialized;

        private void Start()
        {
            Initialize();
        }

        private void Awake()
        {
            if (inputActions == null)
            {
                Debug.LogError("No input actions asset assigned to PlayerInputController!");
            }
        }

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            inputActions.Disable();

            playerActionMap = inputActions.FindActionMap("Player");
            uiActionMap = inputActions.FindActionMap("UI");

            moveAction = playerActionMap.FindAction("Move");
            lookAction = playerActionMap.FindAction("Look");
            primaryAction = playerActionMap.FindAction("Primary");
            secondaryAction = playerActionMap.FindAction("Secondary");
            zoomAction = playerActionMap.FindAction("Zoom");
            jumpAction = playerActionMap.FindAction("Jump");

            inputActions.Enable();

            Cursor.lockState = CursorLockMode.Locked;

            isInitialized = true;
        }

        private void Update()
        {
            if (!isInitialized)
            {
                return;
            }

            moveInput = Vector2.zero;
            lookInput = Vector2.zero;

            moveInput = moveAction.ReadValue<Vector2>();
            lookInput = lookAction.ReadValue<Vector2>();
            jumpInput = jumpAction.IsPressed();

            // Handle player movement
            HandleCharacterInput();

            // Handle player camera
            characterCameraController.HandleLookInputVertically(lookInput.y);
        }

        private void HandleCharacterInput()
        {
            Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
            characterMovementController.HandleMovementInput(moveDirection);
        }
    }
}