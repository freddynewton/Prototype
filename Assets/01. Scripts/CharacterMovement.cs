using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Character
{
    public class CharacterMovement : MonoBehaviour
    {
        public CharacterMovementMotor motor;

        [Header("Stable Movement")]
        [SerializeField]
        private float maxStableMovementSpeed = 8f;

        [Header("Air Movement")]
        [SerializeField]
        private float maxAirMovementSpeed = 10;

        [SerializeField]
        private float airAccelerationSpeed = 15;

        [SerializeField]
        private float drag = 0.1f;

        [Header("Jumping")]
        [SerializeField]
        private float jumpUpSpeed = 10f;

        [SerializeField]
        private float JumpPreGroundingGraceTime = 0.1f;

        [SerializeField]
        private float jumpPostGroundingGraceTime = 0.1f;

        [Header("Misc")]
        [SerializeField]
        private Vector3 gravity = new Vector3(0, -30f, 0);

        public void HandleMovementInput(Vector3 inputDirection)
        {
            if (motor.GroundingStatus.IsStableOnGround)
            {
                HandleStableMovement(inputDirection);
            }
            else
            {
                HandleAirMovement(inputDirection);
            }
        }

        public void HandleJumpInput()
        {
            if (motor.GroundingStatus.IsStableOnGround && motor.GroundingStatus.TimeSinceGrounded <= JumpPreGroundingGraceTime)
            {
                motor.Velocity = new Vector3(motor.Velocity.x, jumpUpSpeed, motor.Velocity.z);
            }
        }

        private void Update()
        {
            HandleGravity();
        }

        private void HandleGravity()
        {
            if (motor.GroundingStatus.IsStableOnGround)
            {
                motor.Velocity -= 20f * Time.deltaTime * Vector3.Project(motor.Velocity, motor.GroundingStatus.GroundNormal);
            }
            else
            {
                motor.Velocity += gravity * Time.deltaTime;
            }
        }

        private void HandleAirMovement(Vector3 inputDirection)
        {
            Vector3 targetMovementVelocity = inputDirection * maxAirMovementSpeed;
            Vector3 velocityDiff = targetMovementVelocity - motor.Velocity;
            Vector3 velocityChange = Vector3.ClampMagnitude(velocityDiff, airAccelerationSpeed * Time.deltaTime);
            motor.Velocity += velocityChange;

            // Gravity
            motor.Velocity += gravity * Time.deltaTime;

            // Drag
            motor.Velocity *= 1f / (1f + (drag * Time.deltaTime));
        }

        private void HandleStableMovement(Vector3 inputDirection)
        {
            // Reorient input direction to move along on the ground
            Vector3 effectiveGroundNormal = motor.GroundingStatus.GroundNormal;
            Vector3 effectiveInputRight = Vector3.Cross(inputDirection, transform.up);
            Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, effectiveInputRight).normalized * inputDirection.magnitude;

            // Calculate target velocity
            Vector3 targetMovementVelocity = reorientedInput * maxStableMovementSpeed;

            // Smooth movement Velocity
            motor.Velocity = Vector3.Lerp(motor.Velocity, targetMovementVelocity, 1f - Mathf.Exp(-20f * Time.deltaTime));
        }
    }
}
