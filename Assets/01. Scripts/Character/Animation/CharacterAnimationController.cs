using Game.Data.CharacterAnimationData;
using KinematicCharacterController;
using Unity.VisualScripting;
using UnityEngine;

namespace Game.Character.Animation
{
    public class CharacterAnimationController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private Animator Animator;

        [SerializeField]
        private CharacterMovementController CharacterController;

        [SerializeField]
        private CharacterCoreController CharacterCoreController;

        private readonly CharacterAnimationParameterData animationParameterData = new();

        private void SetAnimatorParameterValues()
        {
            // World Space to Blend Tree Space
            Vector3 blendTreeSpace = WordSpaceToBlendTreeSpace(CharacterCoreController.GetLookPosition());

            // Set Vertical and Horizontal Values
            Animator.SetFloat(animationParameterData.VerticalHash, blendTreeSpace.z, 0.1f, Time.deltaTime);
            Animator.SetFloat(animationParameterData.HorizontalHash, blendTreeSpace.x, 0.1f, Time.deltaTime);

            // Set Speed Value
            Animator.SetFloat(animationParameterData.SpeedHash, CharacterController.MoveInputAxis.magnitude, 0.1f, Time.deltaTime);

            // Set Turn Values
            Animator.SetFloat(animationParameterData.IdleTurnHash, CharacterCoreController.DesiredRotationAngle() / 180, 0.1f, Time.deltaTime);
            Animator.SetFloat(animationParameterData.MovingTurnHash, CharacterCoreController.DesiredRotationAngle() / 180, 0.1f, Time.deltaTime);

            // Animator.SetFloat(animationParameterData.VerticalHash, CharacterController.MoveInputAxis.x, 0.1f, Time.deltaTime);
            // Animator.SetFloat(animationParameterData.HorizontalHash, CharacterController.MoveInputAxis.z, 0.1f, Time.deltaTime);
        }

        private void Update()
        {
            SetAnimatorParameterValues();
        }

        private void Awake()
        {
            if (Animator == null)
            {
                Debug.LogError("No animator assigned to CharacterAnimationController!");
            }

            if (CharacterController == null)
            {
                Debug.LogError("No character controller assigned to CharacterAnimationController!");
            }

            if (CharacterCoreController == null)
            {
                Debug.LogError("No character core controller assigned to CharacterAnimationController!");
            }
        }

        private Vector3 WordSpaceToBlendTreeSpace(Vector3 LookAtPosition)
        {
            Vector3 inputaxis = CharacterController.MoveInputAxis;

            if (inputaxis.magnitude > 0)
            {
                //Forward Input Value
                Vector3 normalizedLookingAt = LookAtPosition - transform.position;
                normalizedLookingAt.Normalize();

                float forwardBackwardsMagnitude = Mathf.Clamp(Vector3.Dot(inputaxis, normalizedLookingAt), -1, 1);

                //Righ Input Value
                float rightLeftMagnitude = Mathf.Clamp(Vector3.Dot(inputaxis, transform.right), -1, 1);

                return new Vector3(rightLeftMagnitude, 0, forwardBackwardsMagnitude).normalized;
            }
            else
            {
                return inputaxis;
            }
        }

        private void CalculateBodyRotation(ref float bodyRotation)
        {
            if (CharacterController.IsMoving)
            {
                if (CharacterController.IsGrounded)
                {
                    bodyRotation = Mathf.LerpAngle(bodyRotation, CharacterCoreController.DesiredRotationAngle() / 180, 2.5f * Time.deltaTime);

                    if (Mathf.Abs(CharacterCoreController.DesiredRotationAngle()) < 10)
                    {
                        bodyRotation = Mathf.LerpAngle(bodyRotation, 0, 2 * Time.deltaTime);
                    }
                }
                else
                {
                    bodyRotation = Mathf.Lerp(bodyRotation, 0f, 8 * Time.deltaTime);
                }
            }
            else
            {
                bodyRotation = Mathf.Lerp(bodyRotation, 0f, 8 * Time.deltaTime);
            }
        }

        private void CalculateRotationIntensity(ref float RotationIntensity, float Multiplier = 2)
        {
            float diff = Multiplier * Vector3.SignedAngle(transform.forward, Quaternion.Euler(CharacterCoreController.OldEulerAngles) * Vector3.forward, transform.up);

            RotationIntensity = Mathf.LerpAngle(RotationIntensity, diff, 5 * Time.deltaTime);

            CharacterCoreController.OldEulerAngles = transform.eulerAngles;
        }
    }
}
