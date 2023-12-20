using Game.Character;
using Game.Data;
using KinematicCharacterController;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Player
{
    public class PlayerInputController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private InputActionAsset inputActions;

        [SerializeField]
        private Character.CharacterController characterController;

        [SerializeField]
        private CharacterCamera characterCamera;

        // Action Maps
        private InputActionMap playerActionMap;
        private InputActionMap uiActionMap;

        // Actions
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction primaryAction;
        private InputAction secondaryAction;
        private InputAction zoomAction;

        // Input Values
        private Vector2 lookInput;
        private Vector2 moveInput;
        private float zoomInput;

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

            inputActions.Enable();

            Cursor.lockState = CursorLockMode.Locked;

            // Tell camera to follow transform
            characterCamera.SetFollowTransform(characterController.CameraFollowPoint);

            // Ignore the character's collider(s) for camera obstruction checks
            characterCamera.IgnoredColliders.Clear();
            characterCamera.IgnoredColliders.AddRange(characterController.GetComponentsInChildren<Collider>());

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
            zoomInput = 0f;

            moveInput = moveAction.ReadValue<Vector2>();
            lookInput = lookAction.ReadValue<Vector2>();
            // zoomInput = -zoomAction.ReadValue<float>();

            // Handle player movement
            HandleCharacterInput();
        }

        private void LateUpdate()
        {
            if (!isInitialized || Cursor.lockState != CursorLockMode.Locked)
            {
                return;
            }

            // Handle rotating the camera along with physics movers
            if (characterCamera.RotateWithPhysicsMover && characterController.Motor.AttachedRigidbody != null)
            {
                characterCamera.PlanarDirection = characterController.Motor.AttachedRigidbody.GetComponent<PhysicsMover>().RotationDeltaFromInterpolation * characterCamera.PlanarDirection;
                characterCamera.PlanarDirection = Vector3.ProjectOnPlane(characterCamera.PlanarDirection, characterController.Motor.CharacterUp).normalized;
            }

            // Apply inputs to the camera
            characterCamera.UpdateWithInput(Time.deltaTime, zoomInput, lookInput);
        }

        private void HandleCharacterInput()
        {
            PlayerCharacterInputs characterInputs = new()
            {
                // Build the CharacterInputs struct
                MoveAxisForward = moveInput.y,
                MoveAxisRight = moveInput.x,
                CameraRotation = characterCamera.Transform.rotation
            };

            // Apply inputs to character
            characterController.SetInputs(ref characterInputs);
        }
    }
}